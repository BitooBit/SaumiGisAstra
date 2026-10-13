Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Threading

' Smoke: вызов Linux Save-диалога из процесса Wine без Redirect (файл-результат).
Module ExcelLinuxSaveSmoke
    Sub Main()
        Dim code As Integer = Run()
        Environment.Exit(code)
    End Sub

    Function Run() As Integer
        Console.WriteLine("ExcelLinuxSaveSmoke start")
        Dim prefix As String = Environment.GetEnvironmentVariable("WINEPREFIX")
        If String.IsNullOrEmpty(prefix) Then
            Console.WriteLine("FAIL: WINEPREFIX empty")
            Return 2
        End If

        Dim exportLinux As String = prefix.TrimEnd("/"c, "\"c) & "/drive_c/SAUMI/export"
        Dim suggested As String = exportLinux & "/smoke_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".xlsx"
        Dim outFile As String = prefix.TrimEnd("/"c, "\"c) & "/logs/excel_linux_save_smoke_out.txt"

        EnsureWineDir(prefix & "/drive_c/SAUMI/export")
        EnsureWineDir(prefix & "/logs")
        TryDeleteWineFile(outFile)

        Dim chooser As String
        Dim fake As String = Environment.GetEnvironmentVariable("SAUMI_SAVE_SMOKE_FAKE")
        If fake = "1" Then
            chooser = WriteFakeChooser(prefix)
            Console.WriteLine("mode=FAKE")
        Else
            chooser = "/tmp/saumi_excel_save_dialog.sh"
            If Not WineFileExists(chooser) Then
                chooser = prefix.TrimEnd("/"c, "\"c) & "/bin/saumi_excel_save_dialog.sh"
            End If
            Console.WriteLine("mode=GUI")
        End If
        Console.WriteLine("chooser=" & chooser)
        Console.WriteLine("suggested=" & suggested)

        Dim args As String = "/unix /bin/bash " & chooser & " " & suggested & " " & outFile
        ' Важно: start "" — пустой title, иначе первая кавычка съедается как заголовок
        Dim psi As New ProcessStartInfo()
        psi.FileName = "cmd.exe"
        psi.Arguments = "/c start /wait """" " & args
        psi.UseShellExecute = False
        psi.CreateNoWindow = True
        Console.WriteLine("cmd=" & psi.Arguments)
        Dim p As Process = Process.Start(psi)
        If p Is Nothing Then
            Console.WriteLine("FAIL: Process.Start")
            Return 4
        End If
        p.WaitForExit(180000)
        Console.WriteLine("cmd_exit=" & p.ExitCode.ToString())

        Dim body As String = WaitResult(outFile, 60)
        If String.IsNullOrEmpty(body) Then
            Console.WriteLine("FAIL: no result file")
            Return 5
        End If
        Console.WriteLine("result=" & body)
        If body = "CANCEL" Then
            Console.WriteLine("PASS CANCEL")
            Return 0
        End If
        If body.StartsWith("FAIL:") Then
            Console.WriteLine("FAIL chooser")
            Return 6
        End If
        If body.IndexOf("export") < 0 AndAlso body.IndexOf(".xlsx") < 0 Then
            Console.WriteLine("FAIL unexpected path")
            Return 7
        End If
        Dim winePath As String = "Z:" & body.Replace("/"c, "\"c)
        Console.WriteLine("winePath=" & winePath)
        Console.WriteLine("PASS")
        Return 0
    End Function

    Function WriteFakeChooser(ByVal prefix As String) As String
        Dim unix As String = prefix.TrimEnd("/"c, "\"c) & "/logs/saumi_save_fake.sh"
        Dim wine As String = "Z:" & unix.Replace("/"c, "\"c)
        File.WriteAllText(wine, "#!/bin/bash" & ChrW(10) & "echo ""$1"" > ""$2""" & ChrW(10) & "chmod +x ""$2"" 2>/dev/null" & ChrW(10))
        Return unix
    End Function

    Sub EnsureWineDir(ByVal unixDir As String)
        Dim wine As String = "Z:" & unixDir.Replace("/"c, "\"c)
        Directory.CreateDirectory(wine)
    End Sub

    Sub TryDeleteWineFile(ByVal unixFile As String)
        Try
            Dim wine As String = "Z:" & unixFile.Replace("/"c, "\"c)
            If File.Exists(wine) Then File.Delete(wine)
        Catch
        End Try
    End Sub

    Function WineFileExists(ByVal unixFile As String) As Boolean
        Try
            Return File.Exists("Z:" & unixFile.Replace("/"c, "\"c))
        Catch
            Return False
        End Try
    End Function

    Function WaitResult(ByVal unixFile As String, ByVal sec As Integer) As String
        Dim wine As String = "Z:" & unixFile.Replace("/"c, "\"c)
        Dim i As Integer
        For i = 1 To sec * 5
            Try
                If File.Exists(wine) Then
                    Dim t As String = File.ReadAllText(wine).Trim()
                    If t.Length > 0 Then Return t
                End If
            Catch
            End Try
            Thread.Sleep(200)
        Next
        Return Nothing
    End Function

    Function Quote(ByVal s As String) As String
        Return """" & s & """"
    End Function
End Module
