# EventHub Backend API

**Version 1.4**

A comprehensive event management platform built with ASP.NET Core 8.0, featuring role-based access control, real-time notifications, Telegram integration, advanced user management, and comprehensive activity logging.

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
- **User impersonation** for administrative purposes

### 📅 Event Management
- **CRUD operations** for events (Create, Read, Update, Delete)
- **Event categorization** with expanded categories:
  - Technology, Business, Education, Entertainment, Sports, Health
  - Conference, Workshop, Seminar, Party, Concert, Exhibition, Networking
  - Meetup, Social, Other
- **Date and time management** with timezone support
- **Location tracking** and venue information
- **Participant limits** and capacity management
- **Event status tracking** (upcoming, ongoing, completed)
- **Organizer assignment** and ownership management
- **Event filtering** by category, date, location, and organizer

### 👥 User Management
- **User registration** and profile management
- **Role assignment** and permission management with hierarchical validation
- **User banning system** with duration-based restrictions and reason tracking
- **User muting system** with rank-based restrictions
- **Profile updates** and personal information management
- **Ownership transfer** with Telegram verification system
- **Role hierarchy enforcement** (Owner > SeniorAdmin > Admin > Organizer > User)
- **Organizer blacklist** for managing banned users per organizer
- **Preferred language** settings for internationalization

### 💬 Social Features
- **Event comments** with threaded replies
- **Comment moderation** (pin/unpin, edit, delete)
- **Reaction system** with emoji support
- **Favorite events** bookmarking
- **Event planning** (RSVP functionality)
- **Social interactions** tracking
- **Comment editing** with edit history tracking

### 🔔 Notification System
- **Real-time notifications** for various events
- **In-app notification center** with read/unread status
- **Telegram integration** for external notifications
- **Event reminders** and updates
- **Comment notifications** for event organizers
- **Reaction notifications** for event creators
- **Notification management** (mark as read, delete, bulk operations)

### 📊 Activity Logging & Analytics
- **Comprehensive audit logs** for all user actions
- **Activity tracking** with detailed metadata
- **Administrative analytics** and reporting
- **User behavior monitoring** and analysis
- **System usage statistics**
- **Real-time activity monitoring** with filtering capabilities
- **Detailed audit trail** for security and compliance
- **Activity log pagination** and search functionality
- **User-specific activity logs** for targeted monitoring

### 🤖 Telegram Integration
- **Telegram bot** for external notifications
- **User verification** via Telegram with 6-digit codes
- **Event reminders** sent to Telegram
- **Database access confirmation** via Telegram codes
- **Secure code generation** and verification
- **Telegram ID linking** to user accounts
- **Ownership transfer verification** with enhanced security
- **Critical operation verification** for administrative functions

### 🛡️ Security Features
- **CORS protection** with frontend-specific policies
- **Input validation** and sanitization
- **SQL injection prevention** via Entity Framework
- **XSS protection** with proper content encoding
- **Rate limiting** and abuse prevention
- **Secure password policies** and validation
- **Rank-based restrictions** for user management
- **Role validation** and hierarchical enforcement

### 🗄️ Database Features
- **PostgreSQL** with advanced querying capabilities
- **Entity Framework Core** for ORM functionality
- **Database migrations** for schema management
- **Connection pooling** and optimization
- **Data integrity** constraints and relationships
- **Backup and recovery** support
- **Indexing optimization** for performance

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
- `BansController` - User banning system with reason tracking
- `MutesController` - User muting system with rank validation
- `RoleController` - Role management and assignment
- `ActivityLogController` - Comprehensive audit logging and monitoring
- `OrganizerBlacklistController` - Organizer-specific blacklists
- `OwnershipController` - Ownership transfer operations with verification
- `BotController` - Telegram bot integration
- `SeedController` - Database seeding operations

### Services
- `JwtService` - JWT token generation and validation
- `UserService` - User business logic and operations
- `ActivityLogService` - Audit logging functionality
- `NotificationService` - Notification creation and management
- `NotificationHostedService` - Background notification processing

### Models
- `User` - User entity with roles, permissions, and preferences
- `Event` - Event entity with all event-related data
- `EventComment` - Comment system entities with moderation features
- `PostReaction` - Reaction system entities
- `FavoriteEvent` - Event bookmarking entities
- `PlannedEvent` - RSVP system entities
- `UserBanEntry` - User banning records with reason and duration
- `UserMuteEntry` - User muting records with rank validation
- `OrganizerBlacklist` - Organizer blacklist entities
- `ActivityLog` - Comprehensive audit log entries with metadata
- `Notification` - Notification entities with read status
- `TelegramVerification` - Telegram verification records
- `Role` - Role definitions with hierarchical structure

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
- **Pagination support** for large datasets
- **Efficient indexing** for database queries

## 🔒 Security Considerations

- **HTTPS enforcement** in production
- **Secure headers** and security policies
- **Input validation** and sanitization
- **SQL injection prevention**
- **XSS protection**
- **CSRF protection**
- **Rate limiting** implementation
- **Role-based access control**
- **Audit logging** for security compliance

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
- `POST /Auth/impersonate` - User impersonation (admin)

### Events
- `GET /Event` - Get all events with filtering and pagination
- `POST /Event` - Create new event
- `GET /Event/{id}` - Get specific event
- `PUT /Event/{id}` - Update event
- `DELETE /Event/{id}` - Delete event
- `GET /Event/{id}/attendees` - Get event attendees

### Users
- `GET /User/all` - Get all users with pagination (admin)
- `GET /User/{id}` - Get user profile
- `PUT /User/{id}` - Update user
- `POST /User/ban` - Ban user with reason
- `POST /User/unban` - Unban user
- `POST /User/mute` - Mute user with rank validation
- `POST /User/unmute` - Unmute user
- `PUT /User/{id}/roles` - Update user roles

### Comments
- `GET /Comments/{eventId}` - Get event comments with pagination
- `POST /Comments/{eventId}` - Add comment
- `PUT /Comments/{id}` - Update comment
- `DELETE /Comments/{id}` - Delete comment
- `POST /Comments/{id}/pin` - Pin comment
- `POST /Comments/{id}/unpin` - Unpin comment

### Reactions
- `POST /Reactions/{eventId}` - Add reaction
- `DELETE /Reactions/{eventId}` - Remove reaction

### Notifications
- `GET /Notifications` - Get user notifications with pagination
- `PUT /Notifications/{id}/read` - Mark as read
- `DELETE /Notifications/{id}` - Delete notification
- `PUT /Notifications/mark-all-read` - Mark all as read

### Activity Logs
- `GET /ActivityLogs` - Get activity logs with filtering and pagination
- `GET /ActivityLogs/summary` - Get activity statistics
- `GET /ActivityLogs/user/{userId}` - Get user-specific logs

### Ownership Transfer
- `POST /Ownership/request-transfer` - Request ownership transfer
- `POST /Ownership/confirm-transfer` - Confirm transfer with verification code

### Organizer Blacklist
- `POST /OrganizerBlacklist/add` - Add user to organizer blacklist
- `DELETE /OrganizerBlacklist/{userId}` - Remove user from blacklist
- `GET /OrganizerBlacklist` - Get blacklisted users

## 🆕 Version 1.4 Enhancements

### 🌍 Internationalization Support
- **Multi-language support** with localization files
- **Preferred language** settings for users
- **Localized event categories** and UI strings
- **Language-specific content** delivery

### 🔄 Enhanced Ownership Transfer System
- **Telegram Verification** - Secure 6-digit code verification for ownership transfers
- **Transactional Safety** - Database transactions ensure data consistency
- **Role Hierarchy Enforcement** - Automatic role management during transfers
- **Audit Logging** - Complete tracking of all ownership transfer operations
- **Enhanced Security** - Multiple verification steps for critical operations

### 📊 Comprehensive Activity Logging
- **Real-time Logging** - Track all user actions and system events
- **Advanced Filtering** - Filter logs by user, action type, date range
- **Metadata Capture** - Store detailed information about each action
- **Performance Optimized** - Efficient querying with proper indexing
- **User-specific Logs** - Targeted monitoring for specific users
- **Export Capabilities** - Support for log data export

### 🛡️ Enhanced Security Features
- **Rank-based Restrictions** - Prevent muting users with equal or higher rank
- **Role Validation** - Enforce hierarchical role assignment rules
- **Input Validation** - Comprehensive validation for all endpoints
- **Audit Trail** - Complete security audit trail for compliance
- **Ban Reason Tracking** - Store and display reasons for user bans
- **Enhanced CORS** - Improved cross-origin resource sharing policies

### 🔧 API Improvements
- **Enhanced Error Handling** - Better error messages and status codes
- **Pagination Support** - Consistent pagination across all endpoints
- **Search Functionality** - Advanced search capabilities
- **Performance Optimization** - Improved response times and efficiency
- **Better Documentation** - Enhanced Swagger documentation
- **Consistent Response Format** - Standardized API responses

### 📱 Enhanced Telegram Integration
- **Improved Bot Functionality** - Better notification delivery
- **Enhanced Verification** - More secure verification processes
- **Better Error Handling** - Improved error management for Telegram operations
- **User Experience** - Smoother integration with the platform

### 🗄️ Database Improvements
- **Schema Optimizations** - Improved database structure
- **Index Enhancements** - Better query performance
- **Migration System** - Robust database migration management
- **Data Integrity** - Enhanced constraints and relationships

## 🆘 Support

For support and questions, please refer to the project documentation or create an issue in the repository.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📈 Roadmap

### Planned Features
- **Real-time WebSocket** support for live updates
- **Advanced Analytics** dashboard
- **Email Notifications** integration
- **Mobile API** optimizations
- **Advanced Search** with Elasticsearch
- **File Upload** for event images
- **Calendar Integration** (Google Calendar, Outlook)
- **Payment Integration** for paid events
