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
                       │ Infrastructure  │
                       │ (External Deps) │
                       └─────────────────┘
```

### **Project Structure**
- **NPVCalculator.API** - RESTful Web API controllers and configuration
- **NPVCalculator.Application** - Business logic and services
- **NPVCalculator.Domain** - Core business entities and interfaces
- **NPVCalculator.Infrastructure** - Dependency injection and external concerns
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

## **SOLID Principles Implementation**

### **Single Responsibility Principle**
- `NpvCalculatorService`: Only handles NPV calculations
- `ValidationService`: Only handles input validation
- `NpvController`: Only handles HTTP concerns

### **Open/Closed Principle**
- Services can be extended without modification
- New validation rules can be added easily
- Interface-based design allows new implementations

### **Liskov Substitution Principle**
- All implementations are substitutable for their interfaces
- No breaking behavior changes in derived classes

### **Interface Segregation Principle**
- `INpvCalculator`: Focused calculation methods
- `IValidationService`: Focused validation methods
- No unnecessary method dependencies

### **Dependency Inversion Principle**
- High-level modules depend on abstractions
- Dependency injection throughout the application
- Loose coupling between layers

## **Testing Strategy**

### **Unit Tests** (Planned)
- Business logic validation
- NPV calculation accuracy
- Edge case handling
- Validation rule testing

### **Integration Tests** (Planned)
- API endpoint testing
- End-to-end calculation workflows
- Error handling scenarios

## **Performance Considerations**

- **Asynchronous Processing**: CPU-intensive calculations use `Task.Run()`
- **Progress Logging**: Large calculations show progress updates
- **Input Validation**: Prevents excessive computation requests
- **Response Compression**: Optimized data transfer
- **Efficient Algorithms**: Optimized NPV calculation implementation

## **Input Validation**

### **Business Rules**
- Cash flows: At least one required, maximum 1,000 entries
- Discount rates: 0% to 1,000% range
- Rate increment: Minimum 0.01%
- Maximum calculations: 10,000 per request

### **Data Validation**
- Numeric format validation
- Range boundary checking
- Performance limit enforcement
- Business logic validation (e.g., initial investment typically negative)

## **NPV Calculation Reference**

This application implements the standard Net Present Value formula used in financial analysis:

**NPV = Σ[CFt / (1 + r)^t]**

Where:
- **CFt** = Cash flow at time t
- **r** = Discount rate (as decimal)
- **t** = Time period (0, 1, 2, ...)

### **Learn More About NPV**
- [Investopedia NPV Guide](https://www.investopedia.com/terms/n/npv.asp)
- [Corporate Finance Institute NPV Tutorial](https://corporatefinanceinstitute.com/resources/valuation/net-present-value-npv/)

## **Technology Stack**

### **Backend**
- ASP.NET Core 8.0 Web API
- Entity Framework Core (ready for database integration)
- Serilog for structured logging
- Swagger/OpenAPI for documentation

### **Frontend**
- Blazor WebAssembly 8.0
- Bootstrap 5 for styling
- Chart.js for data visualization
- Blazorise component library

### **Development Tools**
- Visual Studio 2022
- Git for version control


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
