# Environment Variables Setup

## Overview

This project uses a `.env` file to store secret tokens and configuration data. This ensures security when uploading code to Git.

## Setup

### 1. Creating .env file

Copy the `.env.example` file to `.env`:

```bash
cp .env.example .env
```

### 2. Filling variables

Open the `.env` file and fill in all variables:

#### Database
```env
DB_HOST=localhost
DB_NAME=eventhub
DB_USERNAME=postgres
DB_PASSWORD=your_password_here
```

#### JWT settings
```env
JWT_KEY=your_jwt_secret_key_here
JWT_ISSUER=EventHub
JWT_AUDIENCE=EventHubClient
JWT_EXPIRE_MINUTES=60
```

#### Telegram settings
```env
TELEGRAM_BOT_TOKEN=your_bot_token_here
TELEGRAM_WEBHOOK_URL=your_webhook_url_here
TELEGRAM_WEBHOOK_SECRET=your_webhook_secret_here
```

### 3. Generating JWT key

To generate a secure JWT key, use:

```bash
# In PowerShell
$bytes = New-Object Byte[] 64
(New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Or in command line
openssl rand -base64 64
```

## Security

- ✅ `.env` file is added to `.gitignore` and won't be uploaded to Git
- ✅ `.env.example` contains examples without real values
- ✅ All secret tokens are now stored in environment variables

## File Structure

```
EventHub/
├── .env                 # Real values (not in Git)
├── .env.example         # Example values (in Git)
├── appsettings.json     # Configuration with environment variables
└── README_ENV.md        # This documentation
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_HOST` | Database host | `localhost` |
| `DB_NAME` | Database name | `eventhub` |
| `DB_USERNAME` | Database user | `postgres` |
| `DB_PASSWORD` | Database password | `your_password` |
| `JWT_KEY` | JWT secret key | `base64_encoded_key` |
| `JWT_ISSUER` | JWT token issuer | `EventHub` |
| `JWT_AUDIENCE` | JWT token audience | `EventHubClient` |
| `JWT_EXPIRE_MINUTES` | Token lifetime (minutes) | `60` |
| `TELEGRAM_BOT_TOKEN` | Telegram bot token | `123456789:ABCdefGHIjklMNOpqrsTUVwxyz` |
| `TELEGRAM_WEBHOOK_URL` | Webhook URL | `https://your-domain.com/api/telegram/webhook` |
| `TELEGRAM_WEBHOOK_SECRET` | Webhook secret | `your_webhook_secret` |

## Deployment

When deploying to server:

1. Create `.env` file on server
2. Fill all variables with real values
3. Ensure `.env` file is not publicly accessible

## Troubleshooting

### "Configuration value not found" Error

Make sure:
- `.env` file exists in project root
- All variables are filled
- No extra spaces in values

### Database Connection Error

Check:
- Correctness of `DB_HOST`, `DB_NAME`, `DB_USERNAME`, `DB_PASSWORD`
- Database availability
- Database user permissions 