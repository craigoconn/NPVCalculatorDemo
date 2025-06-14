# NPV Calculator

A full-stack Net Present Value (NPV) calculator built with **ASP.NET Core Web API** and **Blazor WebAssembly**, demonstrating Clean Architecture principles and modern software engineering practices.

## **Overview**

This application calculates Net Present Value for a series of cash flows across multiple discount rates, providing both tabular results and interactive visualizations. The solution demonstrates API-first design with clear separation between frontend and backend layers.

## **Architecture**

### **Clean Architecture Implementation**
```
┌─────────────────┐    ┌─────────────────┐
│   Blazor WASM   │    │   ASP.NET API   │
│   (Frontend)    │───▶│  (Presentation) │
└─────────────────┘    └─────────────────┘
                                │
                       ┌─────────────────┐
                       │   Application   │
                       │ (Business Logic)│
                       └─────────────────┘
                                │
                       ┌─────────────────┐
                       │     Domain      │
                       │ (Core Business) │
                       └─────────────────┘
                                │
                       ┌─────────────────┐
                       │     Shared      │
                       │ (Common Models) │
                       └─────────────────┘
```

### **Project Structure**
- **NPVCalculator.API** - RESTful Web API controllers and configuration
- **NPVCalculator.Application** - Business logic orchestration and workflow services
- **NPVCalculator.Domain** - Core business entities, interfaces, and calculation logic
- **NPVCalculator.Shared** - Data transfer objects and shared models
- **NPVCalculator.Client** - Blazor WebAssembly frontend application

## **Features**

### **Backend (API)**
- RESTful API design with proper HTTP status codes
- Comprehensive input validation with business rules
- Asynchronous processing for performance
- Structured logging and error handling
- CORS configuration for cross-origin requests
- Swagger/OpenAPI documentation

### **Frontend (SPA)**
- Responsive single-page application
- Interactive forms with real-time validation
- Asynchronous API communication
- Data visualization with Chart.js
- Progressive result display
- Bootstrap-based responsive design

### **NPV Calculation**
- Manual implementation (no external financial libraries)
- Standard NPV formula: **NPV = Σ[CFt / (1 + r)^t]**
- Configurable discount rate ranges and increments
- Support for multiple cash flow periods
- Precision handling for financial calculations

## **Getting Started**

### **Prerequisites**
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Node.js (for Chart.js dependencies)

### **Running the Application**

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/npv-calculator.git
   cd npv-calculator
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the API**
   ```bash
   cd NPVCalculator.API
   dotnet run
   ```
   API will be available at: `https://localhost:7191`

4. **Run the Blazor Client**
   ```bash
   cd NPVCalculator.Client
   dotnet run
   ```
   Frontend will be available at: `https://localhost:5002`


## 🔌 **API Endpoints**

### **Calculate NPV**
```http
POST /api/npv/calculate
Content-Type: application/json

{
  "cashFlows": [-1000, 300, 400, 500],
  "lowerBoundRate": 1.00,
  "upperBoundRate": 15.00,
  "rateIncrement": 0.25
}
```

**Response:**
```json
[
  {
    "rate": 1.00,
    "value": 158.49
  },
  {
    "rate": 1.25,
    "value": 147.32
  }
  // ... more results
]
```

### **API Documentation**
Visit `https://localhost:7191/swagger` for interactive API documentation.

## **Frontend Usage**

1. **Enter Cash Flows**: Input comma-separated values (e.g., `-1000,300,400,500`)
2. **Set Rate Range**: Configure lower bound, upper bound, and increment
3. **Calculate**: Click "Calculate NPV" to process
4. **View Results**: See tabular data and interactive chart
5. **Analyze**: Green rows indicate positive NPV (profitable investments)


## **Testing Strategy**

### **Unit Tests**
Run backend tests:
```bash
cd NPVCalculator.API.Tests
dotnet test

cd NPVCalculator.Application.Tests
dotnet test

cd NPVCalculator.Domain.Tests
dotnet test
```

Run frontend tests:
```bash
cd NPVCalculator.Client.Tests
dotnet test
```

### **Test Coverage**
- Business logic validation and NPV calculation accuracy
- API endpoint testing with mocked dependencies
- Component rendering and user interaction testing
- HTTP service testing with various response scenarios
- Input validation and error handling

## **Performance Considerations**

- **Asynchronous Processing**: CPU-intensive calculations use `Task.Run()` with periodic yielding
- **Input Validation**: Prevents excessive computation requests
- **Response Compression**: Optimized data transfer

## **Input Validation**

### **Business Rules**
- Cash flows: At least one required, maximum 1,000 entries
- Discount rates: -100% to 1,000% range
- Rate increment: Minimum 0.01%
- Maximum calculations: 10,000 per request
- Individual cash flow values: Cannot exceed ±$1 trillion

### **Validation Features**
- Numeric format validation
- Range boundary checking
- Performance limit enforcement
- Business logic warnings (e.g., initial investment typically negative)
- Client-side and server-side validation

## **NPV Calculation Reference**

This application implements the standard Net Present Value formula used in financial analysis:

**NPV = Σ[CFt / (1 + r)^t]**

Where:
- **CFt** = Cash flow at time t
- **r** = Discount rate (as decimal)
- **t** = Time period (0, 1, 2, ...)

### **Sample Calculation**

**Input:**
- Cash Flows: [-1000, 300, 400, 500]
- Discount Rate: 10%

**Calculation:**
```
NPV = -1000/(1+0.10)⁰ + 300/(1+0.10)¹ + 400/(1+0.10)² + 500/(1+0.10)³
NPV = -1000/1 + 300/1.10 + 400/1.21 + 500/1.331
NPV = -1000 + 272.73 + 330.58 + 375.66
NPV = -21.03
```

**Result:** Negative NPV indicates the investment may not be profitable at 10% discount rate.

### **Learn More About NPV**
- [Investopedia NPV Guide](https://www.investopedia.com/terms/n/npv.asp)
- [Corporate Finance Institute NPV Tutorial](https://corporatefinanceinstitute.com/resources/valuation/net-present-value-npv/)

## **Technology Stack**

### **Backend**
- ASP.NET Core 8.0 Web API
- Microsoft.Extensions.Logging for structured logging
- Swashbuckle.AspNetCore for API documentation
- System.Text.Json for serialization

### **Frontend**
- Blazor WebAssembly 8.0
- Bootstrap 5 for responsive styling
- Chart.js for data visualization
- System.Net.Http.Json for API communication

### **Testing**
- xUnit testing framework
- FluentAssertions for readable assertions
- Moq for mocking dependencies
- bUnit for Blazor component testing

## **Project Components**

### **Backend Services**
- **NpvDomainService**: Pure mathematical NPV calculation logic (Domain layer)
- **NpvCalculatorService**: Workflow orchestration and batch processing (Application layer)
- **ValidationService**: Input validation and business rules (Application layer)
- **NpvController**: HTTP request handling and response formatting (API layer)

### **Frontend Components**
- **NpvInputForm**: User input form with validation
- **NpvResults**: Results table with color-coded values
- **NpvChart**: Interactive Chart.js visualization
- **Calculate**: Main page component orchestrating the workflow

### **Frontend Services**
- **NpvCalculationService**: Orchestrates calculation workflow
- **NpvService**: HTTP client for API communication
- **ChartService**: Chart.js integration and rendering
- **InputValidationService**: Client-side input validation
- 
## **Sample Calculation**

**Input:**
- Cash Flows: [-1000, 300, 400, 500]
- Discount Rate: 10%

**Calculation:**
```
NPV = -1000/(1+0.10)⁰ + 300/(1+0.10)¹ + 400/(1+0.10)² + 500/(1+0.10)³
NPV = -1000/1 + 300/1.10 + 400/1.21 + 500/1.331
NPV = -1000 + 272.73 + 330.58 + 375.66
NPV = -21.03
```

**Result:** Negative NPV indicates the investment may not be profitable at 10% discount rate.
