Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

' COM-прокси Excel для SAUMI под Wine.
' Xlsx.22: имя файла — ComboBox (редактор), KeyPreview формы; без zenity.
' mmp32 не трогаем. Managed=Y не используем.
' Win32 GetSaveFileName — только запасной.
' hide / ExplorerWindow не трогаем.

Namespace SaumiExcelProxy

    <ComVisible(True)> _
    <Guid("00024500-0000-0000-C000-000000000046")> _
    <ProgId("Excel.Application")> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class ExcelApplication
        ' ASCII-маркер для проверки сборки (strings на Linux)
        Public Const ProxyBuild As String = "SaumiExcelProxy.Xlsx.23"

        Private _visible As Boolean
        Private _exportAttempted As Boolean
        Private _workbooks As Workbooks
        Friend ActiveWorkbook As Workbook
        ' Для Autofit → export (Visible больше не блокируем диалогом)
        Friend Shared Current As ExcelApplication

        Public ReadOnly Property Version() As String
            Get
                Return "16.0"
            End Get
        End Property

        ' SAUMI: после Visible сразу ReleaseComObject. Диалог здесь → 80004002 и закрытие окна поиска.
        ' Экспорт перенесён в Columns.Autofit (последний вызов перед Visible).
        Public Property Visible() As Boolean
            Get
                Return _visible
            End Get
            Set(ByVal value As Boolean)
                _visible = value
                Log("Visible=" & value.ToString() & " (export via Autofit)")
            End Set
        End Property

        Friend Sub RunExportOnce(ByVal reason As String)
            If _exportAttempted Then
                Log(reason & " -> export skip (already attempted)")
                Return
            End If
            If ActiveWorkbook Is Nothing Then
                Log(reason & " -> export skip (no workbook)")
                Return
            End If
            _exportAttempted = True
            Try
                Log(reason & " -> export")
                ActiveWorkbook.ExportToFile()
            Catch ex As Exception
                Log("EXPORT FAIL: " & ex.GetType().FullName & " " & ex.Message)
            End Try
        End Sub

        Public Property DisplayAlerts() As Boolean
            Get
                Return True
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property

        Public Property ScreenUpdating() As Boolean
            Get
                Return True
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property

        Public ReadOnly Property Workbooks() As Workbooks
            Get
                Return _workbooks
            End Get
        End Property

        Public Sub New()
            Try
                Current = Me
                _workbooks = New Workbooks(Me)
                Log("ctor " & ProxyBuild)
            Catch ex As Exception
                ' не роняем поток экспорта
                Try
                    Log("ctor FAIL: " & ex.Message)
                Catch
                End Try
            End Try
        End Sub

        Public Sub Quit()
            Log("Quit")
        End Sub

        ' Каталог DLL (C:\SAUMI) — экспорт/маркеры рядом с прокси
        Friend Shared Function SaumiDir() As String
            Try
                Dim loc As String = Assembly.GetExecutingAssembly().Location
                If Not String.IsNullOrEmpty(loc) Then
                    Return Path.GetDirectoryName(loc)
                End If
            Catch
            End Try
            Return "C:\SAUMI"
        End Function

        ' Единое место логов с Wine/hide: ~/Saumi/logs через Z:\ (= / под Wine)
        Friend Shared Function LogsDir() As String
            Try
                Dim prefix As String = Environment.GetEnvironmentVariable("WINEPREFIX")
                If Not String.IsNullOrEmpty(prefix) Then
                    Dim z As String = "Z:" & prefix.Replace("/"c, "\"c)
                    If Not z.EndsWith("\") Then z = z & "\"
                    Return z & "logs"
                End If
            Catch
            End Try
            Try
                Dim home As String = Environment.GetEnvironmentVariable("HOME")
                If Not String.IsNullOrEmpty(home) Then
                    Dim z As String = "Z:" & home.Replace("/"c, "\"c)
                    If Not z.EndsWith("\") Then z = z & "\"
                    Return z & "Saumi\logs"
                End If
            Catch
            End Try
            Return Path.Combine(SaumiDir(), "logs")
        End Function

        Friend Shared Sub Log(ByVal msg As String)
            Try
                Dim dir As String = LogsDir()
                If Not Directory.Exists(dir) Then
                    Directory.CreateDirectory(dir)
                End If
                Dim logPath As String = Path.Combine(dir, "excel_proxy.log")
                File.AppendAllText(logPath, _
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " " & msg & Environment.NewLine)
            Catch
            End Try
        End Sub
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class Workbooks
        Private ReadOnly _app As ExcelApplication

        Public Sub New(ByVal app As ExcelApplication)
            _app = app
        End Sub

        Public Function Add() As Workbook
            Return AddWorkbook()
        End Function

        Public Function Add(ByVal Template As Object) As Workbook
            Return AddWorkbook()
        End Function

        Private Function AddWorkbook() As Workbook
            ExcelApplication.Log("Workbooks.Add")
            Dim wb As New Workbook(_app)
            _app.ActiveWorkbook = wb
            Return wb
        End Function

        Public ReadOnly Property Count() As Integer
            Get
                If _app.ActiveWorkbook Is Nothing Then Return 0
                Return 1
            End Get
        End Property
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class Workbook
        Private ReadOnly _app As ExcelApplication
        Private ReadOnly _worksheets As Worksheets

        Public Sub New(ByVal app As ExcelApplication)
            _app = app
            _worksheets = New Worksheets()
        End Sub

        Public ReadOnly Property Worksheets() As Worksheets
            Get
                Return _worksheets
            End Get
        End Property

        Public Sub Close(ByVal SaveChanges As Object)
            ExcelApplication.Log("Close save=" & Convert.ToString(SaveChanges))
        End Sub

        Friend Sub ExportToFile()
            Dim filePath As String = AskSavePath()
            If String.IsNullOrEmpty(filePath) Then
                ExcelApplication.Log("Export cancelled by user")
                Return
            End If
            Dim ext As String = IO.Path.GetExtension(filePath)
            If String.IsNullOrEmpty(ext) Then
                filePath = filePath & ".xlsx"
            ElseIf String.Equals(ext, ".xls", StringComparison.OrdinalIgnoreCase) _
                OrElse String.Equals(ext, ".xml", StringComparison.OrdinalIgnoreCase) Then
                ' всегда настоящий Excel Open XML
                filePath = IO.Path.ChangeExtension(filePath, ".xlsx")
            End If
            Dim dir As String = IO.Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(dir) Then
                Directory.CreateDirectory(dir)
            End If
            _worksheets.ActiveSheet.SaveSpreadsheet(filePath)
            ExcelApplication.Log("Export OK " & filePath)
            Try
                Dim markerDir As String = IO.Path.Combine(ExcelApplication.SaumiDir(), "export")
                Directory.CreateDirectory(markerDir)
                File.WriteAllText(IO.Path.Combine(markerDir, "LAST_EXPORT.txt"), filePath & Environment.NewLine)
            Catch
            End Try
        End Sub

        ' Save-диалог на STA (экспорт SAUMI — MTA-фон)
        Private _askResult As String
        Private _askError As Exception

        Private Function AskSavePath() As String
            Try
                Dim autoPath As String = Environment.GetEnvironmentVariable("SAUMI_EXCEL_SAVE_PATH")
                If Not String.IsNullOrEmpty(autoPath) Then
                    ExcelApplication.Log("Save AUTO " & autoPath)
                    Return autoPath
                End If
            Catch
            End Try
            _askResult = Nothing
            _askError = Nothing
            Dim t As New Thread(New ThreadStart(AddressOf AskSavePathSta))
            t.SetApartmentState(ApartmentState.STA)
            t.IsBackground = True
            t.Start()
            If Not t.Join(180000) Then
                ExcelApplication.Log("Save dialog timeout — fallback")
                Return DefaultExportPath()
            End If
            If _askError IsNot Nothing Then
                ExcelApplication.Log("Save dialog FAIL: " & _askError.Message & " — fallback")
                Return DefaultExportPath()
            End If
            Return _askResult
        End Function

        Private Sub AskSavePathSta()
            Try
                Dim defName As String = "saumi_export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xlsx"
                Dim exportDir As String = IO.Path.Combine(ExcelApplication.SaumiDir(), "export")
                Directory.CreateDirectory(exportDir)
                Dim startDir As String = DialogStartDir(exportDir)
                ExcelApplication.Log("FolderSave InitialDir=" & startDir)
                Dim chosen As String = FolderSaveDialog.Show(startDir, defName)
                If chosen Is Nothing Then
                    _askResult = Nothing
                    ExcelApplication.Log("FolderSave Cancel")
                Else
                    _askResult = chosen
                    ExcelApplication.Log("FolderSave OK " & _askResult)
                End If
            Catch ex As Exception
                ExcelApplication.Log("FolderSave FAIL: " & ex.Message & " — fallback Win32")
                Try
                    Dim defName2 As String = "saumi_export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xlsx"
                    Dim exportDir2 As String = IO.Path.Combine(ExcelApplication.SaumiDir(), "export")
                    Dim startDir2 As String = DialogStartDir(exportDir2)
                    Dim chosen2 As String = NativeSaveDialog.Show(startDir2, defName2)
                    If String.IsNullOrEmpty(chosen2) Then
                        _askResult = Nothing
                        ExcelApplication.Log("GetSaveFileName Cancel")
                    Else
                        _askResult = chosen2
                        ExcelApplication.Log("GetSaveFileName OK " & _askResult)
                    End If
                Catch ex2 As Exception
                    _askError = ex2
                End Try
            End Try
        End Sub

        ' Путь для запасного Win32: Z:\home\<user>\Saumi\drive_c\SAUMI\export
        Private Shared Function DialogStartDir(ByVal exportDirWin As String) As String
            Try
                Dim prefix As String = Environment.GetEnvironmentVariable("WINEPREFIX")
                If Not String.IsNullOrEmpty(prefix) Then
                    Dim z As String = "Z:" & prefix.Replace("/"c, "\"c)
                    If Not z.EndsWith("\") Then z = z & "\"
                    Dim exportZ As String = z & "drive_c\SAUMI\export"
                    Try
                        Directory.CreateDirectory(exportZ)
                    Catch
                    End Try
                    Return exportZ
                End If
            Catch
            End Try
            Try
                Dim home As String = Environment.GetEnvironmentVariable("HOME")
                If Not String.IsNullOrEmpty(home) Then
                    Return "Z:" & home.Replace("/"c, "\"c)
                End If
            Catch
            End Try
            Return exportDirWin
        End Function

        Private Shared Function DefaultExportPath() As String
            Dim exportDir As String = IO.Path.Combine(ExcelApplication.SaumiDir(), "export")
            Directory.CreateDirectory(exportDir)
            Return IO.Path.Combine(exportDir, _
                "saumi_export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xlsx")
        End Function
    End Class

    ' Свой Save As: список папок через System.IO (Z:\); имя — редактируемый ComboBox.
    Friend NotInheritable Class FolderSaveDialog
        Private Sub New()
        End Sub

        Public Shared Function Show(ByVal initialDir As String, ByVal defaultFileName As String) As String
            Dim f As New SaveAsFolderForm(initialDir, defaultFileName)
            Dim r As DialogResult = f.ShowDialog()
            If r = DialogResult.OK Then
                Return f.SelectedPath
            End If
            Return Nothing
        End Function
    End Class

    Friend Class SaveAsFolderForm
        Inherits Form

        Private Shared ReadOnly SaumiBlue As Color = Color.FromArgb(0, 102, 179)
        Private Shared ReadOnly SaumiBlueDark As Color = Color.FromArgb(0, 74, 140)
        Private Shared ReadOnly SaumiBg As Color = Color.FromArgb(245, 248, 252)

        Private ReadOnly _folderIcon As Image
        Private ReadOnly _upIcon As Image
        Private ReadOnly _header As Panel
        Private ReadOnly _titleLbl As Label
        Private ReadOnly _pathLabel As Label
        Private ReadOnly _list As ListBox
        Private ReadOnly _nameBox As ComboBox
        Private ReadOnly _btnUp As Button
        Private ReadOnly _btnSave As Button
        Private ReadOnly _btnCancel As Button
        Private _dir As String
        Public SelectedPath As String

        Private Class FolderEntry
            Public Name As String
            Public FullPath As String
            Public Overrides Function ToString() As String
                Return Name
            End Function
        End Class

        Public Sub New(ByVal initialDir As String, ByVal defaultFileName As String)
            Me.Text = "Сохранить Excel"
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.MinimizeBox = False
            Me.MaximizeBox = False
            Me.ShowInTaskbar = False
            Me.ClientSize = New Size(580, 460)
            Me.BackColor = SaumiBg
            Me.Font = SaumiUiFont(9.0F, FontStyle.Regular)
            ' Диалог поверх SAUMI, чтобы клавиши не уходили в mmp32 (не путать с Managed=Y).
            Me.TopMost = True
            Me.KeyPreview = True

            _folderIcon = MakeFolderIcon()
            _upIcon = MakeUpArrowIcon()

            _dir = initialDir
            If String.IsNullOrEmpty(_dir) Then _dir = "Z:\"
            Try
                Directory.CreateDirectory(_dir)
            Catch
            End Try

            _header = New Panel()
            _header.Location = New Point(0, 0)
            _header.Size = New Size(580, 48)
            _header.BackColor = SaumiBlue
            _header.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

            _titleLbl = New Label()
            _titleLbl.Text = "SAUMI  ·  Сохранить Excel"
            _titleLbl.ForeColor = Color.White
            _titleLbl.Font = SaumiUiFont(11.0F, FontStyle.Bold)
            _titleLbl.Location = New Point(16, 12)
            _titleLbl.AutoSize = True
            _header.Controls.Add(_titleLbl)

            _pathLabel = New Label()
            _pathLabel.Location = New Point(16, 58)
            _pathLabel.Size = New Size(430, 22)
            _pathLabel.ForeColor = SaumiBlueDark
            _pathLabel.Font = SaumiUiFont(8.5F, FontStyle.Regular)
            _pathLabel.AutoEllipsis = True

            _btnUp = New Button()
            _btnUp.Text = " Вверх"
            _btnUp.Image = _upIcon
            _btnUp.TextImageRelation = TextImageRelation.ImageBeforeText
            _btnUp.ImageAlign = ContentAlignment.MiddleLeft
            _btnUp.TextAlign = ContentAlignment.MiddleLeft
            _btnUp.Location = New Point(452, 54)
            _btnUp.Size = New Size(92, 28)
            _btnUp.FlatStyle = FlatStyle.Flat
            _btnUp.BackColor = Color.White
            _btnUp.ForeColor = SaumiBlueDark
            _btnUp.Font = SaumiUiFont(8.5F, FontStyle.Regular)
            _btnUp.FlatAppearance.BorderColor = SaumiBlue
            AddHandler _btnUp.Click, AddressOf OnUp

            ' OwnerDraw: иконки рисуем сами — ListView+ImageList под Wine часто без картинок
            _list = New ListBox()
            _list.Location = New Point(16, 90)
            _list.Size = New Size(548, 270)
            _list.TabIndex = 1
            _list.DrawMode = DrawMode.OwnerDrawFixed
            _list.ItemHeight = 26
            _list.IntegralHeight = False
            _list.BackColor = Color.White
            _list.BorderStyle = BorderStyle.FixedSingle
            _list.Font = SaumiUiFont(9.5F, FontStyle.Regular)
            AddHandler _list.DrawItem, AddressOf OnListDraw
            AddHandler _list.DoubleClick, AddressOf OnListDbl
            AddHandler _list.KeyDown, AddressOf OnListKey

            Dim nameLbl As New Label()
            nameLbl.Text = "Имя файла"
            nameLbl.Location = New Point(16, 372)
            nameLbl.AutoSize = True
            nameLbl.ForeColor = SaumiBlueDark
            nameLbl.Font = SaumiUiFont(8.5F, FontStyle.Regular)
            nameLbl.Cursor = Cursors.IBeam
            AddHandler nameLbl.Click, AddressOf FocusNameCaretEnd

            _nameBox = New ComboBox()
            _nameBox.Location = New Point(16, 392)
            _nameBox.Size = New Size(320, 26)
            _nameBox.TabIndex = 0
            _nameBox.TabStop = True
            _nameBox.DropDownStyle = ComboBoxStyle.DropDown
            _nameBox.Enabled = True
            _nameBox.Font = SaumiUiFont(10.0F, FontStyle.Regular)
            _nameBox.Text = defaultFileName
            _nameBox.IntegralHeight = False
            _nameBox.MaxDropDownItems = 1
            _nameBox.AutoCompleteMode = AutoCompleteMode.None
            _nameBox.FlatStyle = FlatStyle.Standard

            _btnSave = New Button()
            _btnSave.Text = "Сохранить"
            _btnSave.Location = New Point(344, 388)
            _btnSave.Size = New Size(112, 32)
            _btnSave.TabIndex = 2
            _btnSave.FlatStyle = FlatStyle.Flat
            _btnSave.BackColor = SaumiBlue
            _btnSave.ForeColor = Color.White
            _btnSave.Font = SaumiUiFont(9.0F, FontStyle.Bold)
            _btnSave.FlatAppearance.BorderSize = 0
            _btnSave.DialogResult = DialogResult.None
            AddHandler _btnSave.Click, AddressOf OnSave

            _btnCancel = New Button()
            _btnCancel.Text = "Отмена"
            _btnCancel.Location = New Point(464, 388)
            _btnCancel.Size = New Size(96, 32)
            _btnCancel.TabIndex = 3
            _btnCancel.FlatStyle = FlatStyle.Flat
            _btnCancel.BackColor = Color.White
            _btnCancel.ForeColor = SaumiBlueDark
            _btnCancel.Font = SaumiUiFont(9.0F, FontStyle.Regular)
            _btnCancel.FlatAppearance.BorderColor = SaumiBlue
            _btnCancel.DialogResult = DialogResult.Cancel

            Me.Controls.Add(_header)
            Me.Controls.Add(_pathLabel)
            Me.Controls.Add(_btnUp)
            Me.Controls.Add(_list)
            Me.Controls.Add(nameLbl)
            Me.Controls.Add(_nameBox)
            Me.Controls.Add(_btnSave)
            Me.Controls.Add(_btnCancel)
            Me.AcceptButton = _btnSave
            Me.CancelButton = _btnCancel
            AddHandler Me.Shown, AddressOf OnFormShown
            AddHandler Me.KeyDown, AddressOf OnFormKeyDown
            AddHandler Me.KeyPress, AddressOf OnFormKeyPress
            AddHandler Me.Activated, AddressOf OnFormActivated

            RefreshList()
        End Sub

        Private Sub OnFormShown(ByVal sender As Object, ByVal e As EventArgs)
            BeginInvoke(New MethodInvoker(AddressOf FocusNameCaretEndInvoked))
        End Sub

        Private Sub OnFormActivated(ByVal sender As Object, ByVal e As EventArgs)
            FocusNameCaretEnd(sender, e)
        End Sub

        Private Sub FocusNameCaretEndInvoked()
            FocusNameCaretEnd(Me, EventArgs.Empty)
        End Sub

        Private Sub OnFormKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
            If e.KeyCode = Keys.F2 Then
                FocusNameCaretEnd(sender, e)
                e.Handled = True
                Return
            End If
            ' Редактируем имя на уровне формы — TextBox под Wine часто не получает WM_CHAR
            If EditNameByKey(e) Then
                e.Handled = True
                e.SuppressKeyPress = True
            End If
        End Sub

        Private Sub OnFormKeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs)
            If Char.IsControl(e.KeyChar) Then Return
            InsertIntoName(e.KeyChar.ToString())
            e.Handled = True
        End Sub

        Private Function EditNameByKey(ByVal e As KeyEventArgs) As Boolean
            Try
                Dim t As String = _nameBox.Text
                Dim i As Integer = _nameBox.SelectionStart
                Dim n As Integer = _nameBox.SelectionLength
                If e.KeyCode = Keys.Back Then
                    If n > 0 Then
                        _nameBox.Text = t.Remove(i, n)
                        _nameBox.SelectionStart = i
                    ElseIf i > 0 Then
                        _nameBox.Text = t.Remove(i - 1, 1)
                        _nameBox.SelectionStart = i - 1
                    End If
                    _nameBox.SelectionLength = 0
                    Return True
                End If
                If e.KeyCode = Keys.Delete Then
                    If n > 0 Then
                        _nameBox.Text = t.Remove(i, n)
                    ElseIf i < t.Length Then
                        _nameBox.Text = t.Remove(i, 1)
                    End If
                    _nameBox.SelectionStart = i
                    _nameBox.SelectionLength = 0
                    Return True
                End If
                If e.KeyCode = Keys.Left Then
                    If i > 0 Then _nameBox.SelectionStart = i - 1
                    _nameBox.SelectionLength = 0
                    Return True
                End If
                If e.KeyCode = Keys.Right Then
                    If i < t.Length Then _nameBox.SelectionStart = i + 1
                    _nameBox.SelectionLength = 0
                    Return True
                End If
                If e.KeyCode = Keys.Home Then
                    _nameBox.SelectionStart = 0
                    _nameBox.SelectionLength = 0
                    Return True
                End If
                If e.KeyCode = Keys.End Then
                    _nameBox.SelectionStart = t.Length
                    _nameBox.SelectionLength = 0
                    Return True
                End If
            Catch
            End Try
            Return False
        End Function

        Private Sub InsertIntoName(ByVal s As String)
            Try
                Dim t As String = _nameBox.Text
                Dim i As Integer = _nameBox.SelectionStart
                Dim n As Integer = _nameBox.SelectionLength
                If n > 0 Then t = t.Remove(i, n)
                t = t.Insert(i, s)
                _nameBox.Text = t
                _nameBox.SelectionStart = i + s.Length
                _nameBox.SelectionLength = 0
            Catch
            End Try
        End Sub

        ' Фокус + каретка в конец. Без SelectAll.
        Private Sub FocusNameCaretEnd(ByVal sender As Object, ByVal e As EventArgs)
            Try
                If Not _nameBox.Focused Then _nameBox.Focus()
                _nameBox.SelectionLength = 0
                _nameBox.SelectionStart = _nameBox.Text.Length
            Catch
            End Try
        End Sub
        ' Verdana — штатный шрифт SAUMI (corefonts); запас — Tahoma / MS Sans Serif
        Private Shared Function SaumiUiFont(ByVal size As Single, ByVal style As FontStyle) As Font
            Dim names() As String = {"Verdana", "Tahoma", "Microsoft Sans Serif", "Arial"}
            Dim n As String
            For Each n In names
                Try
                    Return New Font(n, size, style)
                Catch
                End Try
            Next
            Return New Font(FontFamily.GenericSansSerif, size, style)
        End Function

        Private Shared Function MakeFolderIcon() As Image
            Dim bmp As New Bitmap(20, 20)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.Clear(Color.Transparent)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Dim tab() As Point = { _
                    New Point(2, 5), New Point(8, 5), New Point(10, 7), New Point(2, 7)}
                Using brTab As New SolidBrush(Color.FromArgb(255, 200, 80))
                    g.FillPolygon(brTab, tab)
                End Using
                Using brBody As New SolidBrush(Color.FromArgb(255, 196, 64))
                    g.FillRectangle(brBody, 2, 7, 16, 11)
                End Using
                Using pen As New Pen(Color.FromArgb(180, 120, 20))
                    g.DrawRectangle(pen, 2, 7, 16, 11)
                    g.DrawLine(pen, 2, 7, 10, 7)
                End Using
            End Using
            Return bmp
        End Function

        Private Shared Function MakeUpArrowIcon() As Image
            Dim bmp As New Bitmap(16, 16)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.Clear(Color.Transparent)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Dim arrow() As Point = { _
                    New Point(8, 2), New Point(14, 9), New Point(10, 9), _
                    New Point(10, 14), New Point(6, 14), New Point(6, 9), New Point(2, 9)}
                Using br As New SolidBrush(SaumiBlue)
                    g.FillPolygon(br, arrow)
                End Using
                Using pen As New Pen(SaumiBlueDark)
                    g.DrawPolygon(pen, arrow)
                End Using
            End Using
            Return bmp
        End Function

        Private Sub OnListDraw(ByVal sender As Object, ByVal e As DrawItemEventArgs)
            e.DrawBackground()
            If e.Index < 0 OrElse e.Index >= _list.Items.Count Then Return
            Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
            Dim bg As Color = SaumiBlue
            Dim fg As Color = Color.White
            If Not selected Then
                bg = Color.White
                fg = Color.FromArgb(30, 40, 55)
            End If
            Using br As New SolidBrush(bg)
                e.Graphics.FillRectangle(br, e.Bounds)
            End Using
            Dim iconY As Integer = e.Bounds.Y + (e.Bounds.Height - _folderIcon.Height) \ 2
            e.Graphics.DrawImage(_folderIcon, e.Bounds.X + 6, iconY)
            Dim textRect As New Rectangle(e.Bounds.X + 30, e.Bounds.Y, e.Bounds.Width - 36, e.Bounds.Height)
            TextRenderer.DrawText(e.Graphics, _list.Items(e.Index).ToString(), _list.Font, textRect, fg, _
                TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            If selected Then e.DrawFocusRectangle()
        End Sub

        Private Sub RefreshList()
            _pathLabel.Text = _dir
            _list.BeginUpdate()
            _list.Items.Clear()
            Try
                Dim di As New DirectoryInfo(_dir)
                Dim subdirs() As DirectoryInfo = di.GetDirectories()
                Dim si As Integer, sj As Integer
                For si = 0 To subdirs.Length - 2
                    For sj = si + 1 To subdirs.Length - 1
                        If String.Compare(subdirs(si).Name, subdirs(sj).Name, True) > 0 Then
                            Dim tmpDi As DirectoryInfo = subdirs(si)
                            subdirs(si) = subdirs(sj)
                            subdirs(sj) = tmpDi
                        End If
                    Next
                Next
                Dim d As DirectoryInfo
                For Each d In subdirs
                    Dim fe As New FolderEntry()
                    fe.Name = d.Name
                    fe.FullPath = d.FullName
                    _list.Items.Add(fe)
                Next
            Catch ex As Exception
                _list.Items.Add("(не удалось прочитать: " & ex.Message & ")")
            End Try
            _list.EndUpdate()
        End Sub

        Private Sub OnUp(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim parent As DirectoryInfo = Directory.GetParent(_dir)
                If parent IsNot Nothing Then
                    _dir = parent.FullName
                    RefreshList()
                End If
            Catch
            End Try
        End Sub

        Private Sub OnListDbl(ByVal sender As Object, ByVal e As EventArgs)
            EnterSelectedFolder()
        End Sub

        Private Sub OnListKey(ByVal sender As Object, ByVal e As KeyEventArgs)
            If e.KeyCode = Keys.Enter Then
                EnterSelectedFolder()
                e.Handled = True
            End If
        End Sub

        Private Sub EnterSelectedFolder()
            If _list.SelectedItem Is Nothing Then Return
            Dim fe As FolderEntry = TryCast(_list.SelectedItem, FolderEntry)
            If fe Is Nothing Then Return
            Try
                If Directory.Exists(fe.FullPath) Then
                    _dir = fe.FullPath
                    RefreshList()
                End If
            Catch
            End Try
        End Sub

        Private Sub OnSave(ByVal sender As Object, ByVal e As EventArgs)
            Dim fn As String = _nameBox.Text.Trim()
            If fn.Length = 0 Then
                MessageBox.Show("Укажите имя файла", "Сохранить Excel")
                Return
            End If
            If Not fn.ToLower().EndsWith(".xlsx") Then fn = fn & ".xlsx"
            Try
                SelectedPath = Path.Combine(_dir, fn)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Сохранить Excel")
            End Try
        End Sub
    End Class

    Friend NotInheritable Class NativeSaveDialog
        Private Sub New()
        End Sub

        Private Const OFN_OVERWRITEPROMPT As Integer = &H2
        Private Const OFN_PATHMUSTEXIST As Integer = &H800
        Private Const OFN_HIDEREADONLY As Integer = &H4
        Private Const OFN_NOCHANGEDIR As Integer = &H8
        Private Const OFN_EXPLORER As Integer = &H80000

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
        Private Structure OPENFILENAME
            Public lStructSize As Integer
            Public hwndOwner As IntPtr
            Public hInstance As IntPtr
            Public lpstrFilter As String
            Public lpstrCustomFilter As String
            Public nMaxCustFilter As Integer
            Public nFilterIndex As Integer
            Public lpstrFile As String
            Public nMaxFile As Integer
            Public lpstrFileTitle As String
            Public nMaxFileTitle As Integer
            Public lpstrInitialDir As String
            Public lpstrTitle As String
            Public Flags As Integer
            Public nFileOffset As Short
            Public nFileExtension As Short
            Public lpstrDefExt As String
            Public lCustData As IntPtr
            Public lpfnHook As IntPtr
            Public lpTemplateName As String
        End Structure

        <DllImport("comdlg32.dll", CharSet:=CharSet.Unicode, EntryPoint:="GetSaveFileNameW", ExactSpelling:=True)> _
        Private Shared Function GetSaveFileNameW(ByRef ofn As OPENFILENAME) As Boolean
        End Function

        <DllImport("user32.dll")> _
        Private Shared Function GetForegroundWindow() As IntPtr
        End Function

        Public Shared Function Show(ByVal initialDir As String, ByVal defaultFileName As String) As String
            ' Только имя файла в lpstrFile — полный путь под Wine часто делает поле имени read-only.
            Dim fileBuf As String = defaultFileName
            If fileBuf Is Nothing OrElse fileBuf.Length = 0 Then fileBuf = "export.xlsx"
            Try
                fileBuf = Path.GetFileName(fileBuf)
            Catch
            End Try
            Const maxFile As Integer = 520
            If fileBuf.Length >= maxFile Then fileBuf = fileBuf.Substring(0, maxFile - 1)
            fileBuf = fileBuf & New String(ChrW(0), maxFile - fileBuf.Length)

            Dim titleBuf As String = New String(ChrW(0), 260)

            Dim ofn As New OPENFILENAME()
            ofn.lStructSize = Marshal.SizeOf(GetType(OPENFILENAME))
            Try
                ofn.hwndOwner = GetForegroundWindow()
            Catch
                ofn.hwndOwner = IntPtr.Zero
            End Try
            ofn.lpstrFilter = "Книга Excel (*.xlsx)" & ChrW(0) & "*.xlsx" & ChrW(0) & ChrW(0)
            ofn.nFilterIndex = 1
            ofn.lpstrFile = fileBuf
            ofn.nMaxFile = maxFile
            ofn.lpstrFileTitle = titleBuf
            ofn.nMaxFileTitle = 260
            ofn.lpstrInitialDir = initialDir
            ofn.lpstrTitle = "Сохранить Excel"
            ofn.Flags = OFN_OVERWRITEPROMPT Or OFN_HIDEREADONLY
            ofn.lpstrDefExt = "xlsx"

            If Not GetSaveFileNameW(ofn) Then
                Return Nothing
            End If
            Dim result As String = ofn.lpstrFile
            If result Is Nothing Then Return Nothing
            Dim z As Integer = result.IndexOf(ChrW(0))
            If z >= 0 Then result = result.Substring(0, z)
            result = result.Trim()
            If result.Length = 0 Then Return Nothing
            Try
                If result.IndexOf("\"c) < 0 AndAlso result.IndexOf("/"c) < 0 _
                    AndAlso Not String.IsNullOrEmpty(initialDir) Then
                    result = Path.Combine(initialDir, result)
                End If
            Catch
            End Try
            Return result
        End Function
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class Worksheets
        Private ReadOnly _sheets As New List(Of Worksheet)
        Private _activeSheet As Worksheet

        Public Sub New()
            _sheets.Add(New Worksheet())
            _activeSheet = _sheets(0)
        End Sub

        Public ReadOnly Property ActiveSheet() As Worksheet
            Get
                Return _activeSheet
            End Get
        End Property

        ' SAUMI: InvokeMember("Item", GetProperty, …, {1})
        Default Public ReadOnly Property Item(ByVal index As Object) As Worksheet
            Get
                Dim i As Integer = ToIndex(index)
                While _sheets.Count <= i
                    _sheets.Add(New Worksheet())
                End While
                _activeSheet = _sheets(i)
                Return _activeSheet
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                If _sheets.Count > 0 Then Return _sheets.Count
                Return 1
            End Get
        End Property

        Private Shared Function ToIndex(ByVal index As Object) As Integer
            Dim i As Integer = 0
            If TypeOf index Is Integer Then
                i = CInt(index) - 1
            ElseIf TypeOf index Is Short Then
                i = CShort(index) - 1
            ElseIf TypeOf index Is String Then
                i = 0
            Else
                Try
                    i = Convert.ToInt32(index, CultureInfo.InvariantCulture) - 1
                Catch
                    i = 0
                End Try
            End If
            If i < 0 Then i = 0
            Return i
        End Function
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class Worksheet
        Private ReadOnly _cells As New Dictionary(Of String, Object)
        Private _name As String = "Sheet1"

        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                If value Is Nothing Then
                    _name = "Sheet1"
                Else
                    _name = value
                End If
                ExcelApplication.Log("sheet=" & _name)
            End Set
        End Property

        ' SAUMI: GetProperty "Cells" с 2 аргументами
        Public ReadOnly Property Cells(ByVal RowIndex As Object, ByVal ColumnIndex As Object) As Range
            Get
                Return New Range(_cells, ToInt(RowIndex, 1), ToInt(ColumnIndex, 1))
            End Get
        End Property

        ' SAUMI: GetProperty "Range" с 2 аргументами (ячейки)
        Public ReadOnly Property Range(ByVal Cell1 As Object, ByVal Cell2 As Object) As Range
            Get
                Dim a As Range = TryCast(Cell1, Range)
                Dim b As Range = TryCast(Cell2, Range)
                If a IsNot Nothing AndAlso b IsNot Nothing Then
                    Return SaumiExcelProxy.Range.FromArea(_cells, a.Row, a.Col, b.Row, b.Col)
                End If
                Return New Range(_cells)
            End Get
        End Property

        Public ReadOnly Property Columns() As ExcelColumns
            Get
                Return New ExcelColumns()
            End Get
        End Property

        ' SAUMI: GetProperty "Rows" с 1 аргументом ("1:10")
        Public ReadOnly Property Rows(ByVal RowIndex As Object) As ExcelRows
            Get
                Return New ExcelRows(Convert.ToString(RowIndex, CultureInfo.InvariantCulture))
            End Get
        End Property

        Friend Sub SaveSpreadsheet(ByVal path As String)
            XlsxWriter.Write(_cells, _name, path)
        End Sub

        Private Shared Function ToInt(ByVal o As Object, ByVal def As Integer) As Integer
            If o Is Nothing Then Return def
            If TypeOf o Is Integer Then Return CInt(o)
            If TypeOf o Is Short Then Return CShort(o)
            Try
                Return Convert.ToInt32(o, CultureInfo.InvariantCulture)
            Catch
                Return def
            End Try
        End Function
    End Class

    ' Минимальный .xlsx (Office Open XML) без сторонних библиотек — ZIP store
    Friend NotInheritable Class XlsxWriter
        Private Sub New()
        End Sub

        Public Shared Sub Write(ByVal cells As Dictionary(Of String, Object), ByVal sheetName As String, ByVal path As String)
            If String.IsNullOrEmpty(sheetName) Then sheetName = "Sheet1"
            sheetName = SanitizeSheetName(sheetName)

            Dim maxR As Integer = 0
            Dim maxC As Integer = 0
            Dim r As Integer, c As Integer
            For Each kv As KeyValuePair(Of String, Object) In cells
                If kv.Key.StartsWith("__") Then Continue For
                ParseCell(kv.Key, r, c)
                If r > maxR Then maxR = r
                If c > maxC Then maxC = c
            Next

            Dim sheetXml As String = BuildSheetXml(cells, maxR, maxC)
            Dim entries As New Dictionary(Of String, Byte())
            entries.Add("[Content_Types].xml", Utf8(ContentTypesXml()))
            entries.Add("_rels/.rels", Utf8(RelsXml()))
            entries.Add("xl/workbook.xml", Utf8(WorkbookXml(sheetName)))
            entries.Add("xl/_rels/workbook.xml.rels", Utf8(WorkbookRelsXml()))
            entries.Add("xl/worksheets/sheet1.xml", Utf8(sheetXml))
            entries.Add("xl/styles.xml", Utf8(StylesXml()))
            ZipStore.Write(path, entries)
        End Sub

        Private Shared Function SanitizeSheetName(ByVal name As String) As String
            Dim bad As Char() = {"\"c, "/"c, "?"c, "*"c, "["c, "]"c, ":"c}
            Dim s As String = name
            Dim i As Integer
            For i = 0 To bad.Length - 1
                s = s.Replace(bad(i), "_"c)
            Next
            If s.Length > 31 Then s = s.Substring(0, 31)
            If s.Length = 0 Then s = "Sheet1"
            Return s
        End Function

        Private Shared Function BuildSheetXml(ByVal cells As Dictionary(Of String, Object), ByVal maxR As Integer, ByVal maxC As Integer) As String
            Dim sb As New StringBuilder()
            Dim r As Integer, c As Integer
            sb.Append("<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>")
            sb.Append("<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">")
            If maxC < 1 Then maxC = 1
            Dim colW(maxC) As Double
            Dim ci As Integer
            For ci = 1 To maxC
                colW(ci) = 12
            Next
            For Each kv As KeyValuePair(Of String, Object) In cells
                If kv.Key.StartsWith("__") Then Continue For
                ParseCell(kv.Key, r, c)
                If c < 1 OrElse c > maxC Then Continue For
                Dim s As String = Convert.ToString(kv.Value, CultureInfo.InvariantCulture)
                If s Is Nothing Then s = ""
                Dim ww As Double = s.Length * 1.1 + 2
                If ww > 55 Then ww = 55
                If ww > colW(c) Then colW(c) = ww
            Next
            sb.Append("<cols>")
            For ci = 1 To maxC
                sb.Append("<col min=""").Append(ci.ToString()).Append(""" max=""").Append(ci.ToString())
                sb.Append(""" width=""").Append(colW(ci).ToString("0.0", CultureInfo.InvariantCulture))
                sb.Append(""" customWidth=""1""/>")
            Next
            sb.Append("</cols>")
            sb.Append("<sheetData>")
            For r = 1 To maxR
                sb.Append("<row r=""").Append(r.ToString()).Append(""">")
                For c = 1 To maxC
                    Dim key As String = r.ToString() & "," & c.ToString()
                    Dim v As Object = Nothing
                    If Not cells.TryGetValue(key, v) OrElse v Is Nothing Then
                        Continue For
                    End If
                    Dim cellRef As String = ColName(c) & r.ToString()
                    Dim fmt As String = ""
                    Dim fo As Object = Nothing
                    If cells.TryGetValue("__fmt__" & key, fo) AndAlso fo IsNot Nothing Then
                        fmt = Convert.ToString(fo, CultureInfo.InvariantCulture)
                        If fmt Is Nothing Then fmt = ""
                    End If
                    If TypeOf v Is Double OrElse TypeOf v Is Single OrElse TypeOf v Is Decimal _
                       OrElse TypeOf v Is Integer OrElse TypeOf v Is Long OrElse TypeOf v Is Short _
                       OrElse TypeOf v Is Byte OrElse TypeOf v Is Boolean Then
                        Dim num As String = Convert.ToString(v, CultureInfo.InvariantCulture)
                        If TypeOf v Is Boolean Then
                            If CBool(v) Then num = "1" Else num = "0"
                            sb.Append("<c r=""").Append(cellRef).Append(""" t=""b""><v>").Append(num).Append("</v></c>")
                        ElseIf LooksLikeDateFormat(fmt) Then
                            sb.Append("<c r=""").Append(cellRef).Append(""" s=""1""><v>").Append(Xml(num)).Append("</v></c>")
                        Else
                            sb.Append("<c r=""").Append(cellRef).Append("""><v>").Append(Xml(num)).Append("</v></c>")
                        End If
                    ElseIf TypeOf v Is DateTime Then
                        Dim dt As DateTime = CType(v, DateTime)
                        Dim serial As Double = dt.ToOADate()
                        sb.Append("<c r=""").Append(cellRef).Append(""" s=""1""><v>") _
                          .Append(serial.ToString(CultureInfo.InvariantCulture)).Append("</v></c>")
                    Else
                        Dim s As String = Convert.ToString(v, CultureInfo.InvariantCulture)
                        If s Is Nothing Then s = ""
                        sb.Append("<c r=""").Append(cellRef).Append(""" t=""inlineStr""><is><t>") _
                          .Append(Xml(s)).Append("</t></is></c>")
                    End If
                Next
                sb.Append("</row>")
            Next
            sb.Append("</sheetData>")
            Dim merges As New List(Of String)
            For Each kv As KeyValuePair(Of String, Object) In cells
                If Not kv.Key.StartsWith("__merge__") Then Continue For
                Dim spec As String = kv.Key.Substring(9)
                Dim parts As String() = spec.Split(":"c)
                If parts.Length <> 2 Then Continue For
                Dim a As String() = parts(0).Split(","c)
                Dim b As String() = parts(1).Split(","c)
                If a.Length <> 2 OrElse b.Length <> 2 Then Continue For
                Dim r1 As Integer, c1 As Integer, r2 As Integer, c2 As Integer
                If Not Integer.TryParse(a(0), r1) Then Continue For
                If Not Integer.TryParse(a(1), c1) Then Continue For
                If Not Integer.TryParse(b(0), r2) Then Continue For
                If Not Integer.TryParse(b(1), c2) Then Continue For
                If r1 = r2 AndAlso c1 = c2 Then Continue For
                merges.Add(ColName(c1) & r1.ToString() & ":" & ColName(c2) & r2.ToString())
            Next
            If merges.Count > 0 Then
                sb.Append("<mergeCells count=""").Append(merges.Count.ToString()).Append(""">")
                Dim m As String
                For Each m In merges
                    sb.Append("<mergeCell ref=""").Append(m).Append("""/>")
                Next
                sb.Append("</mergeCells>")
            End If
            sb.Append("</worksheet>")
            Return sb.ToString()
        End Function

        Private Shared Function LooksLikeDateFormat(ByVal fmt As String) As Boolean
            If String.IsNullOrEmpty(fmt) Then Return False
            Dim f As String = fmt.ToLowerInvariant()
            Return f.IndexOf("d"c) >= 0 AndAlso f.IndexOf("m"c) >= 0 AndAlso f.IndexOf("y"c) >= 0
        End Function

        Private Shared Function ColName(ByVal col As Integer) As String
            Dim n As Integer = col
            Dim s As String = ""
            While n > 0
                Dim m As Integer = (n - 1) Mod 26
                s = Chr(Asc("A"c) + m) & s
                n = (n - 1) \ 26
            End While
            If s.Length = 0 Then s = "A"
            Return s
        End Function

        Private Shared Function Xml(ByVal s As String) As String
            Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
        End Function

        Private Shared Sub ParseCell(ByVal key As String, ByRef row As Integer, ByRef col As Integer)
            row = 1 : col = 1
            Dim p As String() = key.Split(","c)
            If p.Length >= 2 Then
                Integer.TryParse(p(0), row)
                Integer.TryParse(p(1), col)
            End If
        End Sub

        Private Shared Function Utf8(ByVal s As String) As Byte()
            Return Encoding.UTF8.GetBytes(s)
        End Function

        Private Shared Function ContentTypesXml() As String
            Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & _
                "<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">" & _
                "<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>" & _
                "<Default Extension=""xml"" ContentType=""application/xml""/>" & _
                "<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>" & _
                "<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>" & _
                "<Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>" & _
                "</Types>"
        End Function

        Private Shared Function RelsXml() As String
            Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & _
                "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" & _
                "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>" & _
                "</Relationships>"
        End Function

        Private Shared Function WorkbookXml(ByVal sheetName As String) As String
            Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & _
                "<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" " & _
                "xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">" & _
                "<sheets><sheet name=""" & Xml(sheetName) & """ sheetId=""1"" r:id=""rId1""/></sheets>" & _
                "</workbook>"
        End Function

        Private Shared Function WorkbookRelsXml() As String
            Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & _
                "<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">" & _
                "<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>" & _
                "<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>" & _
                "</Relationships>"
        End Function

        Private Shared Function StylesXml() As String
            ' s=""1"" — формат даты
            Return "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" & _
                "<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">" & _
                "<numFmts count=""1""><numFmt numFmtId=""164"" formatCode=""dd.mm.yyyy""/></numFmts>" & _
                "<fonts count=""1""><font><sz val=""11""/><name val=""Calibri""/></font></fonts>" & _
                "<fills count=""1""><fill><patternFill patternType=""none""/></fill></fills>" & _
                "<borders count=""1""><border/></borders>" & _
                "<cellStyleXfs count=""1""><xf/></cellStyleXfs>" & _
                "<cellXfs count=""2""><xf/><xf numFmtId=""164"" applyNumberFormat=""1""/></cellXfs>" & _
                "</styleSheet>"
        End Function
    End Class

    ' ZIP без сжатия (method=0 STORE) — достаточно для .xlsx
    Friend NotInheritable Class ZipStore
        Private Sub New()
        End Sub

        Public Shared Sub Write(ByVal zipPath As String, ByVal files As Dictionary(Of String, Byte()))
            Dim ms As New MemoryStream()
            Dim offsets As New Dictionary(Of String, Integer)
            Dim crcTable As UInteger() = MakeCrcTable()

            For Each kv As KeyValuePair(Of String, Byte()) In files
                Dim nameBytes As Byte() = Encoding.UTF8.GetBytes(kv.Key.Replace("\"c, "/"c))
                Dim data As Byte() = kv.Value
                Dim crc As UInteger = Crc32(data, crcTable)
                offsets(kv.Key) = CInt(ms.Position)

                ' local file header
                WriteU32(ms, &H4034B50UI) ' signature
                WriteU16(ms, 20) ' version needed
                WriteU16(ms, 0) ' flags
                WriteU16(ms, 0) ' method store
                WriteU16(ms, 0) ' time
                WriteU16(ms, 0) ' date
                WriteU32(ms, crc)
                WriteU32(ms, CUInt(data.Length))
                WriteU32(ms, CUInt(data.Length))
                WriteU16(ms, CUShort(nameBytes.Length))
                WriteU16(ms, 0) ' extra
                ms.Write(nameBytes, 0, nameBytes.Length)
                ms.Write(data, 0, data.Length)
            Next

            Dim centralStart As Integer = CInt(ms.Position)
            For Each kv As KeyValuePair(Of String, Byte()) In files
                Dim nameBytes As Byte() = Encoding.UTF8.GetBytes(kv.Key.Replace("\"c, "/"c))
                Dim data As Byte() = kv.Value
                Dim crc As UInteger = Crc32(data, crcTable)
                WriteU32(ms, &H2014B50UI)
                WriteU16(ms, 20)
                WriteU16(ms, 20)
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU32(ms, crc)
                WriteU32(ms, CUInt(data.Length))
                WriteU32(ms, CUInt(data.Length))
                WriteU16(ms, CUShort(nameBytes.Length))
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU16(ms, 0)
                WriteU32(ms, 0)
                WriteU32(ms, CUInt(offsets(kv.Key)))
                ms.Write(nameBytes, 0, nameBytes.Length)
            Next
            Dim centralEnd As Integer = CInt(ms.Position)
            WriteU32(ms, &H6054B50UI)
            WriteU16(ms, 0)
            WriteU16(ms, 0)
            WriteU16(ms, CUShort(files.Count))
            WriteU16(ms, CUShort(files.Count))
            WriteU32(ms, CUInt(centralEnd - centralStart))
            WriteU32(ms, CUInt(centralStart))
            WriteU16(ms, 0)

            File.WriteAllBytes(zipPath, ms.ToArray())
        End Sub

        Private Shared Sub WriteU16(ByVal ms As MemoryStream, ByVal v As UShort)
            ms.WriteByte(CByte(v And &HFFUS))
            ms.WriteByte(CByte((v >> 8) And &HFFUS))
        End Sub

        Private Shared Sub WriteU32(ByVal ms As MemoryStream, ByVal v As UInteger)
            ms.WriteByte(CByte(v And &HFFUI))
            ms.WriteByte(CByte((v >> 8) And &HFFUI))
            ms.WriteByte(CByte((v >> 16) And &HFFUI))
            ms.WriteByte(CByte((v >> 24) And &HFFUI))
        End Sub

        Private Shared Function MakeCrcTable() As UInteger()
            Dim table(255) As UInteger
            Dim i As Integer, j As Integer
            For i = 0 To 255
                Dim c As UInteger = CUInt(i)
                For j = 0 To 7
                    If (c And 1UI) <> 0UI Then
                        c = (&HEDB88320UI Xor (c >> 1))
                    Else
                        c = c >> 1
                    End If
                Next
                table(i) = c
            Next
            Return table
        End Function

        Private Shared Function Crc32(ByVal data As Byte(), ByVal table As UInteger()) As UInteger
            Dim c As UInteger = &HFFFFFFFFUI
            Dim i As Integer
            For i = 0 To data.Length - 1
                c = table(CInt((c Xor data(i)) And &HFFUI)) Xor (c >> 8)
            Next
            Return c Xor &HFFFFFFFFUI
        End Function
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class Range
        Private ReadOnly _cells As Dictionary(Of String, Object)
        Private ReadOnly _r1 As Integer, _c1 As Integer, _r2 As Integer, _c2 As Integer
        Private ReadOnly _isCell As Boolean

        Friend ReadOnly Property Row() As Integer
            Get
                Return _r1
            End Get
        End Property

        Friend ReadOnly Property Col() As Integer
            Get
                Return _c1
            End Get
        End Property

        Public Sub New(ByVal cells As Dictionary(Of String, Object))
            _cells = cells
            _isCell = False
            _r1 = 1 : _c1 = 1 : _r2 = 1 : _c2 = 1
        End Sub

        Friend Sub New(ByVal cells As Dictionary(Of String, Object), ByVal r As Integer, ByVal c As Integer)
            _cells = cells
            _r1 = r : _r2 = r
            _c1 = c : _c2 = c
            _isCell = True
        End Sub

        Private Sub New(ByVal cells As Dictionary(Of String, Object), _
                        ByVal r1 As Integer, ByVal c1 As Integer, _
                        ByVal r2 As Integer, ByVal c2 As Integer, ByVal area As Boolean)
            _cells = cells
            _r1 = r1 : _c1 = c1 : _r2 = r2 : _c2 = c2
            _isCell = False
        End Sub

        Friend Shared Function FromArea(ByVal cells As Dictionary(Of String, Object), _
                                        ByVal r1 As Integer, ByVal c1 As Integer, _
                                        ByVal r2 As Integer, ByVal c2 As Integer) As Range
            Return New Range(cells, Math.Min(r1, r2), Math.Min(c1, c2), Math.Max(r1, r2), Math.Max(c1, c2), True)
        End Function

        Public Property Value() As Object
            Get
                If Not _isCell Then Return Nothing
                Dim v As Object = Nothing
                _cells.TryGetValue(_r1.ToString() & "," & _c1.ToString(), v)
                Return v
            End Get
            Set(ByVal value As Object)
                SetCell(_r1, _c1, value)
            End Set
        End Property

        Public Property Value2() As Object
            Get
                Return Value
            End Get
            Set(ByVal value As Object)
                SetValue2(value)
            End Set
        End Property

        Public Property MergeCells() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
                ExcelApplication.Log("MergeCells=" & value.ToString() & " " & _r1.ToString() & "," & _c1.ToString() & ":" & _r2.ToString() & "," & _c2.ToString())
                If value Then
                    _cells("__merge__" & _r1.ToString() & "," & _c1.ToString() & ":" & _r2.ToString() & "," & _c2.ToString()) = True
                End If
            End Set
        End Property

        Public Property NumberFormat() As String
            Get
                Dim o As Object = Nothing
                If _cells.TryGetValue("__fmt__" & _r1.ToString() & "," & _c1.ToString(), o) AndAlso o IsNot Nothing Then
                    Return Convert.ToString(o, CultureInfo.InvariantCulture)
                End If
                Return "@"
            End Get
            Set(ByVal value As String)
                Dim r As Integer, c As Integer
                For r = _r1 To _r2
                    For c = _c1 To _c2
                        _cells("__fmt__" & r.ToString() & "," & c.ToString()) = value
                    Next
                Next
            End Set
        End Property

        Public ReadOnly Property Font() As ExcelFont
            Get
                Return New ExcelFont()
            End Get
        End Property

        Private Sub SetValue2(ByVal value As Object)
            Dim arr As Array = TryCast(value, Array)
            If arr IsNot Nothing AndAlso arr.Rank = 2 Then
                Dim rows As Integer = arr.GetLength(0)
                Dim cols As Integer = arr.GetLength(1)
                Dim i As Integer, j As Integer
                For i = 0 To rows - 1
                    For j = 0 To cols - 1
                        SetCell(_r1 + i, _c1 + j, arr.GetValue(i, j))
                    Next
                Next
                ExcelApplication.Log("Value2[" & rows.ToString() & "x" & cols.ToString() & "] @" & _r1.ToString() & "," & _c1.ToString())
                Return
            End If
            Me.Value = value
        End Sub

        Private Sub SetCell(ByVal r As Integer, ByVal c As Integer, ByVal v As Object)
            If v Is Nothing Then Return
            Dim key As String = r.ToString() & "," & c.ToString()
            If TypeOf v Is String Then
                Dim dt As DateTime
                If TryParseSaumiDate(CStr(v), dt) Then
                    _cells(key) = dt
                    If Not _cells.ContainsKey("__fmt__" & key) Then
                        _cells("__fmt__" & key) = "dd.mm.yyyy"
                    End If
                    Return
                End If
            End If
            _cells(key) = v
        End Sub

        Private Shared Function TryParseSaumiDate(ByVal s As String, ByRef dt As DateTime) As Boolean
            If s Is Nothing Then Return False
            Dim t As String = s.Trim()
            If t.Length < 8 Then Return False
            Dim formats() As String = { _
                "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy H:mm:ss", "dd.MM.yyyy", _
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd"}
            Return DateTime.TryParseExact(t, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, dt)
        End Function
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class ExcelFont
        Public Property Bold() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property
        Public Property Italic() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property
        Public Property Strikethrough() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property
        Public Property Underline() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property
        Public Property Color() As Object
            Get
                Return 0
            End Get
            Set(ByVal value As Object)
            End Set
        End Property
        Public Property Size() As Object
            Get
                Return 10
            End Get
            Set(ByVal value As Object)
            End Set
        End Property
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class ExcelColumns
        Public Sub Autofit()
            ExcelApplication.Log("Columns.Autofit")
            ' Диалог здесь (sync). ExplorerWindow патч 3.25 пропускает ReleaseComObject —
            ' иначе Join+Release валил процесс и все окна поиска.
            Dim app As ExcelApplication = ExcelApplication.Current
            If app IsNot Nothing Then
                app.RunExportOnce("Autofit")
            End If
        End Sub
    End Class

    <ComVisible(True)> _
    <ClassInterface(ClassInterfaceType.AutoDual)> _
    Public Class ExcelRows
        Private ReadOnly _addr As String
        Public Sub New(ByVal addr As String)
            If addr Is Nothing Then
                _addr = ""
            Else
                _addr = addr
            End If
        End Sub
        Public Sub Group()
            ExcelApplication.Log("Rows.Group " & _addr)
        End Sub
    End Class

End Namespace
