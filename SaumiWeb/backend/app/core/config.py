from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    app_name: str = "SaumiWeb API"
    app_env: str = "development"
    debug: bool = False
    api_prefix: str = "/api/v1"

    jwt_secret_key: str = "change-me-saumiweb-dev-secret-key-32b"
    jwt_algorithm: str = "HS256"
    access_token_expire_minutes: int = 15
    refresh_token_expire_days: int = 7

    cors_origins: str = "http://localhost:5173,http://127.0.0.1:5173"

    login_rate_limit_per_minute: int = 20

    use_oracle: bool = False
    oracle_dsn: str = "localhost:1521/FREEPDB1"
    oracle_thick_mode: bool = False

    @property
    def cors_origin_list(self) -> list[str]:
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


@lru_cache
def get_settings() -> Settings:
    return Settings()
