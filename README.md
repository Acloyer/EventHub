# EventHub Backend API

<<<<<<< HEAD
**Version 1.2.1**
=======
**Version 1.2**
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c

A comprehensive event management platform built with ASP.NET Core 8.0, featuring role-based access control, real-time notifications, Telegram integration, and advanced user management capabilities.

## 🚀 Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer Tokens with ASP.NET Core Identity
- **External Integrations**: Telegram Bot API
- **Documentation**: Swagger/OpenAPI
- **Security**: BCrypt for password hashing, CORS protection
- **Environment**: DotNetEnv for environment variable management

## 📋 Core Features

### 🔐 Authentication & Authorization
- **JWT-based authentication** with configurable token expiration
- **Role-based access control** with hierarchical permissions:
  - `User` - Basic event participation
  - `Organizer` - Event creation and management
  - `Admin` - User management and system administration
  - `SeniorAdmin` - Advanced administrative functions
  - `Owner` - Full system control and ownership transfer
- **Password security** with BCrypt hashing
- **Token refresh** and validation mechanisms

### 📅 Event Management
- **CRUD operations** for events (Create, Read, Update, Delete)
- **Event categorization** (Conference, Workshop, Meetup, Social, Other)
- **Date and time management** with timezone support
- **Location tracking** and venue information
- **Participant limits** and capacity management
- **Event status tracking** (upcoming, ongoing, completed)
- **Organizer assignment** and ownership management

### 👥 User Management
- **User registration** and profile management
- **Role assignment** and permission management
- **User banning system** with duration-based restrictions
- **User muting system** for temporary restrictions
- **Profile updates** and personal information management
- **User impersonation** for administrative purposes
- **Ownership transfer** between users

### 💬 Social Features
- **Event comments** with threaded replies
- **Comment moderation** (pin/unpin, edit, delete)
- **Reaction system** with emoji support
- **Favorite events** bookmarking
- **Event planning** (RSVP functionality)
- **Social interactions** tracking

### 🔔 Notification System
- **Real-time notifications** for various events
- **In-app notification center** with read/unread status
- **Telegram integration** for external notifications
- **Event reminders** and updates
- **Comment notifications** for event organizers
- **Reaction notifications** for event creators

### 📊 Activity Logging & Analytics
- **Comprehensive audit logs** for all user actions
- **Activity tracking** with IP address and user agent logging
- **Administrative analytics** and reporting
- **User behavior monitoring** and analysis
- **System usage statistics**

### 🤖 Telegram Integration
- **Telegram bot** for external notifications
- **User verification** via Telegram
- **Event reminders** sent to Telegram
- **Database access confirmation** via Telegram codes
- **Secure code generation** and verification
- **Telegram ID linking** to user accounts

### 🛡️ Security Features
- **CORS protection** with frontend-specific policies
- **Input validation** and sanitization
- **SQL injection prevention** via Entity Framework
- **XSS protection** with proper content encoding
- **Rate limiting** and abuse prevention
- **Secure password policies** and validation

### 🗄️ Database Features
- **PostgreSQL** with advanced querying capabilities
- **Entity Framework Core** for ORM functionality
- **Database migrations** for schema management
- **Connection pooling** and optimization
- **Data integrity** constraints and relationships
- **Backup and recovery** support

## 🏗️ Architecture

### Controllers
- `AuthController` - Authentication and user registration
- `UserController` - User management and profile operations
- `EventController` - Event CRUD operations and management
- `CommentsController` - Comment system and moderation
- `ReactionsController` - Event reaction management
- `FavoriteEventsController` - Event bookmarking
- `PlannedEventsController` - RSVP and event planning
- `NotificationController` - Notification management
- `BansController` - User banning system
- `MutesController` - User muting system
- `RoleController` - Role management
- `ActivityLogController` - Audit logging
- `OrganizerBlacklistController` - Organizer-specific blacklists
- `OwnershipController` - Ownership transfer operations
- `BotController` - Telegram bot integration
- `SeedController` - Database seeding operations

### Services
- `JwtService` - JWT token generation and validation
- `UserService` - User business logic and operations
- `ActivityLogService` - Audit logging functionality
- `NotificationService` - Notification creation and management
- `NotificationHostedService` - Background notification processing

### Models
- `User` - User entity with roles and permissions
- `Event` - Event entity with all event-related data
- `EventComment` - Comment system entities
- `PostReaction` - Reaction system entities
- `FavoriteEvent` - Event bookmarking entities
- `PlannedEvent` - RSVP system entities
- `UserBanEntry` - User banning records
- `UserMuteEntry` - User muting records
- `OrganizerBlacklist` - Organizer blacklist entities
- `ActivityLog` - Audit log entries
- `Notification` - Notification entities
- `TelegramVerification` - Telegram verification records
- `Role` - Role definitions

## 🔧 Configuration

### Environment Variables
- `TELEGRAM_BOT_TOKEN` - Telegram bot authentication token
- `ConnectionStrings:DefaultConnection` - PostgreSQL connection string
- `Jwt:Key` - JWT signing key
- `Jwt:Issuer` - JWT issuer
- `Jwt:Audience` - JWT audience

### App Settings
- JWT configuration with expiration settings
- Database connection strings
- CORS policies
- Swagger documentation settings

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database
- Telegram Bot Token (optional)

### Installation
1. Clone the repository
2. Configure environment variables
3. Run database migrations: `dotnet ef database update`
4. Start the application: `dotnet run`

### API Documentation
- Swagger UI available at `/swagger`
- OpenAPI specification at `/swagger/v1/swagger.json`

## 📈 Performance Features

- **Async/await** patterns throughout the application
- **Database connection pooling** for optimal performance
- **Caching strategies** for frequently accessed data
- **Optimized queries** with Entity Framework
- **Background services** for non-blocking operations

## 🔒 Security Considerations

- **HTTPS enforcement** in production
- **Secure headers** and security policies
- **Input validation** and sanitization
- **SQL injection prevention**
- **XSS protection**
- **CSRF protection**
- **Rate limiting** implementation

## 🧪 Testing

- **Unit testing** support with xUnit
- **Integration testing** capabilities
- **API testing** with Swagger
- **Database testing** with in-memory providers

## 📝 API Endpoints

### Authentication
- `POST /Auth/register` - User registration
- `POST /Auth/login` - User login
- `POST /Auth/refresh` - Token refresh

### Events
- `GET /Event` - Get all events with filtering
- `POST /Event` - Create new event
- `GET /Event/{id}` - Get specific event
- `PUT /Event/{id}` - Update event
- `DELETE /Event/{id}` - Delete event

### Users
- `GET /User/all` - Get all users (admin)
- `GET /User/{id}` - Get user profile
- `PUT /User/{id}` - Update user
- `POST /User/ban` - Ban user
- `POST /User/mute` - Mute user

### Comments
- `GET /Comments/{eventId}` - Get event comments
- `POST /Comments/{eventId}` - Add comment
- `PUT /Comments/{id}` - Update comment
- `DELETE /Comments/{id}` - Delete comment

### Reactions
- `POST /Reactions/{eventId}` - Add reaction
- `DELETE /Reactions/{eventId}` - Remove reaction

### Notifications
- `GET /Notifications` - Get user notifications
- `PUT /Notifications/{id}/read` - Mark as read
- `DELETE /Notifications/{id}` - Delete notification

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions, please refer to the project documentation or create an issue in the repository.
