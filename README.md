# Meter Reading System

A full-stack application for uploading and processing meter reading data via CSV files.

## Architecture

This solution follows Clean Architecture principles with clear separation of concerns:

- **Frontend**: Angular 17 application with TypeScript
- **Backend**: .NET 8 Web API with Entity Framework Core
- **Database**: PostgreSQL

## Project Structure

### Backend (.NET 8)

```
MeterReading.API/          # Web API controllers and configuration
├── Controllers/           # API endpoints
├── DTOs/                 # Data transfer objects
└── Program.cs            # Application startup

MeterReading.Domain/       # Core business logic and entities

MeterReading.Infrastructure/ # Data access and external services
├── Data/                 # Entity Framework context and configuration
├── Services/             # Business logic implementation
└── Validation/           # Validation logic and types

MeterReading.Tests/        # Unit tests

```
### Frontend (Angular 17)

```

meter-readings-app/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   └── upload/    # File upload component
│   │   ├── models/        # TypeScript interfaces
│   │   ├── services/      # HTTP services
│   │   └── app.module.ts  # Application module
│   └── index.html         # Main HTML file
└── angular.json           # Angular configuration
```

## Features

### CSV Upload & Validation
- Upload CSV files with meter reading data
- Comprehensive validation of file format and data
- All-or-nothing processing (transaction-based)
- Detailed error reporting

### Data Validation
- **CSV Format**: Validates headers, column count, and structure
- **Account Validation**: Ensures account exists in the system
- **Name Validation**: Ensures name and surname have values and not longer than DB column max
- **Date Validation**: Enforces dd/MM/yyyy HH:mm format
- **Meter Reading Validation**: Values must be 0-99999
- **Duplicate Detection**: Prevents duplicate readings for same account/datetime

### User Interface
- Validation feedback
- Detailed error messages with format examples
- Processing statistics (validated, failed, committed)


## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- PostgreSQL 12+
- Visual Studio/VS Code (recommended)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/DanForest-UK/MeterReadings
   cd meter-reading-system
   ```

2. **Update connection string**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=MeterReadingDB;Username=postgres;Password=your_password;Port=5432"
     }
   }
   ```

3. **Start the API**
   ```bash
   dotnet run
   ```
   API will be available at `https://localhost:5121`

### Frontend Setup

1. **Install dependencies**
   ```bash
   cd meter-readings-app
   npm install
   ```

2. **Start development server**
   ```bash
   ng serve
   ```
   Application will be available at `http://localhost:4200`

## Usage

### CSV File Format

Upload CSV files with the following format:

```csv
AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,1002
2233,22/04/2019 12:25,0323
8766,22/04/2019 12:25,3440
```

**Requirements:**
- Exactly 3 columns with headers: `AccountId`, `MeterReadingDateTime`, `MeterReadValue`
- AccountId: Integer (must exist in system)
- MeterReadingDateTime: Format `dd/MM/yyyy HH:mm`
- MeterReadValue: Integer between 0 and 99999
- No extra columns or trailing commas

### API Endpoints

- `POST /api/meterreadings/meter-reading-uploads` - Upload CSV file
- `GET /test/trigger-cache-update` - Refresh account cache (development)

## Testing

### Backend Tests
```bash
cd MeterReading.Tests
dotnet test
```

### Test Coverage
- Domain validation logic
- CSV parsing and validation
- Error handling scenarios
- Value object behavior

## Development Features

### Caching
- In-memory account cache for fast validation
- Automatic cache refresh when accounts are modified
- Thread-safe atomic operations

### Error Handling
- Functional error handling with `ValidationResult<T>`
- Comprehensive error messages
- Transaction rollback on validation failures

### Domain-Driven Design
- Domain models with value objects
- Separation between domain and infrastructure
- Immutable records for data integrity

## Configuration

### CORS Settings
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200"
    ]
  }
}
```


## Deployment

### Database
1. Update connection string for production
2. Run API to create database and seed test data

### API
1. Build: `dotnet build --configuration Release`
2. Publish: `dotnet publish --configuration Release`
3. Deploy to your hosting platform

### Frontend
1. Build: `ng build --configuration production`
2. Deploy `dist/` folder to web server
3. Update API URL in service if needed

## License

This project is licensed under the MIT License - see the LICENSE file for details.
