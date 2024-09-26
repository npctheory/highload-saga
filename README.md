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
Счетчики непрочитанных сообщений обновляются через саги, которые создаются классом [DialogMessageSaga](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Api/Sagas/DialogMessageSaga.cs). Счетчики хранятся в таблице dialogs. За прочитанность/непрочитанность отдельного сообщения отвечает столбец is_read в таблице dialog_messages. Саги хранятся в таблице SagaData.  
Конфигурация Masstransit в файле [highload-saga/server/Dialogs.Api/DependencyInjection.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Api/DependencyInjection.cs) 
  
Сага вызывается ивентами, которые отправляет на шину один из классов:  
[SendMessageCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/SendMessage/SendMessageCommandHandler.cs)  
[ListMessagesQueryHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Queries/ListMessages/ListMessagesQueryHandler.cs)  
### Отправка сообщения  
Отправка сообщений должна обновлять количество непрочитанных сообщений для получателя. Для отправки сообщения используется класс [SendMessageCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/SendMessage/SendMessageCommandHandler.cs). При успешном выполнении он производит событие MessageSent. Событие MessageSent переводит сагу в состояние CountingUnreadMessages, сага производит событие UnreadMessageCountWentStaleEvent на которое подписан класс [UpdateUnreadMessageCountCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/UpdateUnreadMessageCount/UpdateUnreadMessageCountCommandHandler.cs), который обращается к таблице dialog_messages, вычисляет новое количество непрочитанных и записывает новое количество в диалог в таблице dialogs. После этого производит событие UnreadMessageCountUpToDateEvent, которое переводит сагу в состояние Idle.
### Чтение сообщений  
Чтение сообщений должно обнулять счетчик непрочитанных. Класс [ListMessagesQueryHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Queries/ListMessages/ListMessagesQueryHandler.cs), который отвечает для получение списка сообщений, отправляет на шину событие  MessagesListedEvent. Это событие переводит сагу в состояние MarkingUnreadMessagesAsRead и отправляет на шину сообщение AllMessagesWereReadEvent. Это событие принимает класс [MarkDialogMessagesAsReadCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/MarkDialogMessagesAsRead/MarkDialogMessagesAsReadCommandHandler.cs). Этот класс ставит значение true в колонке is_read для всех имеющих отношение к диалогу прочитанных сообщений в dialog_messages и отправляет на шину сообщение UnreadMessageCountIsZeroEvent. Это сообщение переводит сагу в состояние ResettingUnreadMessageCount, и сага отправляет на шину новое событие UnreadMessagesMarkedAsReadEvent, на которое подписан класс [ResetUnreadMessageCountCommandHandler.cs](https://github.com/npctheory/highload-saga/blob/main/server/Dialogs.Application/Dialogs/Commands/ResetUnreadMessageCount/ResetUnreadMessageCountCommandHandler.cs), который обнуляет счетчик в таблице dialogs и отправляет на шину сообщение UnreadMessageCountUpToDate, которое переводит сагу в состояние Idle.  

Пример работы:  

[saga.webm](https://github.com/user-attachments/assets/50e48c09-6f80-4acf-a01a-907630f5ed12)
