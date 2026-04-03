# CurrencyWatch

Микросервисная система для отслеживания курсов валют ЦБ РФ.

## Запуск

```bash
docker-compose up --build
```

API будет доступно на `http://localhost:5000`.

## Сервисы

- **ApiGateway** — точка входа, маршрутизирует запросы через YARP
- **UserService** — регистрация и аутентификация (JWT)
- **FinanceService** — курсы валют и избранное пользователя
- **CurrencyUpdater.Worker** — фоновый сервис, загружает курсы из [ЦБ РФ](http://www.cbr.ru/scripts/XML_daily.asp)
- **Migration.Runner** — применяет миграции EF Core при старте

## API

Авторизованные endpoint-ы требуют заголовок `Authorization: Bearer <token>`.

### Аутентификация

| Метод | Путь                       | Авторизация | Описание                                   |
|-------|----------------------------|-------------|--------------------------------------------|
| POST  | `/users/api/auth/register` | Нет         | Регистрация                                |
| POST  | `/users/api/auth/login`    | Нет         | Вход в систему посредством логина и пароля |
| POST  | `/users/api/auth/refresh`  | Нет         | Обновление токена                          |
| POST  | `/users/api/auth/logout`   | Да          | Выход и аннулирование токена               |

### Курсы валют

| Метод  | Путь                                       | Описание                              |
|--------|--------------------------------------------|---------------------------------------|
| GET    | `/finance/api/currencies`                  | Получить все курсы                    |
| GET    | `/finance/api/currencies/favorites`        | Получить избранные курсы пользователя |
| POST   | `/finance/api/currencies/favorites/{code}` | Добавить валюту в избранное           |
| DELETE | `/finance/api/currencies/favorites/{code}` | Убрать валюту из избранного           |

Все endpoint-ы курсов требуют авторизацию. `{code}` — ISO 4217 код валюты (`USD`, `EUR`, `CNY`).
