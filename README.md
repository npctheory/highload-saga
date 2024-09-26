### О проекте
Домашнее задание по транзакциям.  
Проект состоит из следующих компонентов:  
* Солюшен .NET в папке ./server, который собирается в два образа: core:local и dialogs:local (контейнеры core и dialogs).
* Dockerfile и сид базы данных postgres в папке ./db, который собирается в образ db:local (контейнер pg_master).
* В папке tests находятся запросы для расширения VSCode REST Client и экспорты коллекций и окружений Postman.
* В docker-compose.yml подключаются Redis, Redis Insight, RabbitMQ, PGAdmin.
### Начало работы
Склонировать проект, сделать cd в корень репозитория и запустить Docker Compose.  
Дождаться статуса healthy на контейнерах postgres.  
```bash
git clone https://github.com/npctheory/highload-saga.git
cd highload-saga
docker compose up --build -d
```
После запуска всех контейнеров добавить в базу таблицу с сагами
```bash
docker exec -it dialogs bash

cd /app

dotnet ef database update --project Dialogs.Infrastructure/Dialogs.Infrastructure.csproj --startup-project Dialogs.Api/Dialogs.Api.csproj
```
### Оркестратор
Счетчики непрочитанных сообщений обновляется через саги, которые создаются классом [DialogMessageSaga](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Api/Sagas/DialogMessageSaga.cs)  
Сага вызывается ивентами, которые вызывает один из двух хэндлеров:  
[SendMessageCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/SendMessage/SendMessageCommandHandler.cs)  
[ListMessagesQueryHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Queries/ListMessages/ListMessagesQueryHandler.cs)  
### Отправка сообщения  
Для отправки сообщения используется класс [SendMessageCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/SendMessage/SendMessageCommandHandler.cs). При успешном выполнении он производит событие MessageSent. Событие MessageSent переводит сагу в состояние CountingUnreadMessages, сага производит событие UnreadMessageCountWentStaleEvent на которое подписан класс [UpdateUnreadMessageCountCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/UpdateUnreadMessageCount/UpdateUnreadMessageCountCommandHandler.cs), который обращается к таблице сообщение, вычисляет новое количество непрочитанных и записывает новое количество в диалог в таблице dialogs. После этого производит событие UnreadMessageCountUpToDateEvent, которое переводит сагу в состояние Idle.

Счетчики хранятся в таблице dialogs.  
Саги хранятся в таблице SagaData.  
Пример работы:  

[saga.webm](https://github.com/user-attachments/assets/50e48c09-6f80-4acf-a01a-907630f5ed12)
