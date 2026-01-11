# Backend Functions Usage Summary

## ✅ All Backend Endpoints and Their Usage

### 1. AuthController
- ✅ **POST /api/Auth/login** - Used in `Login.razor` via `AuthService.LoginAsync()`
- ✅ **POST /api/Auth/register** - Used in `Register.razor` via `AuthService.RegisterAsync()`

### 2. ShipRegistryController
- ✅ **GET /api/ShipRegistry** - Used in `Ships.razor` (LoadShips method)
- ✅ **GET /api/ShipRegistry/{id}** - Used in `Ships.razor` (ViewShipModal)
- ✅ **POST /api/ShipRegistry** - Used in `Ships.razor` (HandleAddShip)
- ✅ **PUT /api/ShipRegistry/{id}** - Used in `Ships.razor` (HandleUpdateShip)
- ✅ **DELETE /api/ShipRegistry/{id}** - Used in `Ships.razor` (DeleteShip)

### 3. LicensesController
- ✅ **GET /api/Licenses** - Used in `Licenses.razor` (LoadLicenses method)
- ✅ **POST /api/Licenses** - Used in `Licenses.razor` (HandleAddLicense)

### 4. InspectionsController
- ✅ **GET /api/Inspections** - Used in `Inspections.razor` (LoadInspections method)
- ⚠️ **GET /api/Inspections/{id}** - NEEDS TO BE ADDED (for viewing inspection details)
- ✅ **POST /api/Inspections** - Used in `Inspections.razor` (HandleAddInspection)
- ⚠️ **PUT /api/Inspections/{id}** - NEEDS TO BE ADDED (for editing inspections)
- ✅ **DELETE /api/Inspections/{id}** - Used in `Inspections.razor` (DeleteInspection)

### 5. AmateurCatchesController
- ✅ **GET /api/AmateurCatches** - Used in `MyCatches.razor` (LoadCatches method)
- ✅ **POST /api/AmateurCatches** - Used in `AddCatch.razor` (HandleSubmit)

### 6. ReportsController
- ✅ **GET /api/Reports/expiring-licenses** - Used in `Reports.razor` (LoadReport1)
- ✅ **GET /api/Reports/amateur-ranking** - Used in `Reports.razor` (LoadReport2)
- ✅ **GET /api/Reports/ship-catch-analysis/{year}** - Used in `Reports.razor` (LoadReport3)
- ✅ **GET /api/Reports/ship-fuel-efficiency/{year}** - Used in `Reports.razor` (LoadReport4)
- ✅ **GET /api/Reports/inspections** - Used in `Reports.razor` (LoadReport5)
- ✅ **GET /api/Reports/fisher-statistics/{year}** - Used in `Reports.razor` (LoadReport6)

## Summary
- **Total Backend Endpoints**: 19
- **Used in Frontend**: 17
- **Missing**: 2 (GET /api/Inspections/{id}, PUT /api/Inspections/{id})

## Assignment Requirements Check
Based on the assignment description, you need:
1. ✅ Ship Registry Management - DONE (all CRUD operations)
2. ✅ License Management - DONE (GET, POST)
3. ✅ Amateur Catches - DONE (GET, POST)
4. ✅ Inspections - DONE (GET, POST, DELETE) - Need to add GET/{id} and PUT/{id}
5. ✅ Reports - DONE (all 4 required reports + 2 additional)
