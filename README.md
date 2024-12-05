Here are short instructions to clone and deploy .NET Azure Function locally:

---

### **1. Clone the Repository**
1. Open a terminal or command prompt.
2. Run:  
   ```bash
   git clone <repository_url>
   cd <repository_folder>
   ```

---

### **2. Install Prerequisites**
1. Install **.NET 6/7/8 SDK** (depending on your Azure Function version).  
   Download: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)  
2. Install **Azure Functions Core Tools**:  
   ```bash
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```
3. Install dependencies:  
   ```bash
   dotnet restore
   ```

---

### **3. Configure Local Settings**
1. Create a `local.settings.json` file in the project root if it doesn't exist.
2. Add required settings, attached is the required `local.settings.json` file

---

### **4. Run Locally**
1. Start the function locally:  
   ```bash
   func start
   ```
2. Test your function:
   - For **HTTP triggers**, use tools like Postman or curl to send requests.
   - For **Timer triggers**, check logs for execution.

---

### **5. Debug in Visual Studio Code**
1. Open the repository folder in Visual Studio Code.
3. Press **F5** to run and debug the function.

---

### **Architecture Overview**

The project consists of two Azure Functions, each serving a distinct purpose:

1. **HTTP-triggered Azure Function**:
   - Handles incoming requests from D365 when the "ready for dispatch" button is clicked.
   - Validates, processes, and transforms the incoming JSON payload into a CSV format.
   - Uploads the transformed CSV file to the specified SFTP server folder (`bluecorp-incoming`, `bluecorp-failed`).

2. **Timer-triggered Azure Function**:
   - Periodically checks the `bluecorp-incoming` folder on the SFTP server.
   - Moves processed files to the appropriate folder:
     - `bluecorp-processed` if processing is successful.

This architecture adheres to Azure standards for scalability, maintainability, and security, while separating concerns into two well-defined functions.

---

### **Components and Workflow**

#### **1. HTTP-Triggered Azure Function (Ingest and Process)**
   - **Trigger**: HTTP POST request from D365.
   - **Responsibilities**:
     - Receive and validate JSON payload.
     - Parse and map the payload data to the required CSV format.
     - Upload the generated CSV file to the `bluecorp-incoming` SFTP folder.

   - **Flow**:
     1. D365 sends an HTTP POST request containing the dispatch information (e.g., sales orders, containers, delivery address).
     2. The function validates the payload and ensures it meets the schema requirements (e.g., control number, payload size).
     3. The JSON is transformed into a CSV format, following the mapping rules.
     4. The CSV file is securely uploaded to the SFTP server using public key authentication.

   - **Technologies**:
     - Azure Functions (HTTP trigger).
     - JSON validation libraries.
     - `SSH.NET` for SFTP file handling.
     - Application Insights for telemetry and logging.

---

#### **2. Timer-Triggered Azure Function (File Management)**
   - **Trigger**: Timer trigger (e.g., every 5 minute).
   - **Responsibilities**:
     - Check the `bluecorp-incoming` folder on the SFTP server.
     - Move files to the appropriate folders based on their processing status:
       - `bluecorp-processed` for successfully processed files.

   - **Flow**:
     1. The function connects to the SFTP server and lists files in the `bluecorp-incoming` folder.
     2. For each file:
        - Determine the processing status (e.g., check against 3PL system logs or simulate processing).
        - Move the file to the corresponding folder:
          - `bluecorp-processed` if successful.
          - `bluecorp-failed` if processing failed.
     3. Log successes and errors for monitoring and debugging.

   - **Technologies**:
     - Azure Functions (Timer trigger).
     - `SSH.NET` for SFTP file handling.
     - Application Insights for telemetry and logging.
     - Azure Redis Cache for controlNumber validation
     - Dependency Injection for loosely coupled code

---

### **Deployment and Integration**

- **Infrastructure**: 
  - Both functions are deployed to an Azure Function App, ensuring they share the same compute resources and environment settings.
  - Managed via Github Actions pipelines for automated build and deployment.

- **Security**:
  - HTTP-triggered function uses no authentication.
  - SFTP server uses public key authentication.

- **Monitoring and Telemetry**:
  - Application Insights tracks logs, performance metrics, and errors.
  - Alerts are configured for failures (e.g., file upload errors, SFTP connectivity issues).

---

### **Benefits of the Architecture**

1. **Separation of Concerns**:
   - The HTTP-triggered function focuses on processing and uploading the dispatch data.
   - The timer-triggered function handles post-upload file movement.

2. **Scalability**:
   - Each function can scale independently based on demand (e.g., high-volume incoming requests or frequent file movements).

3. **Reliability**:
   - Timer-triggered function ensures files are processed or moved even if transient issues occur.
   - Built-in retry mechanisms and logging provide resilience and traceability.

4. **Maintainability**:
   - Functions are modular, making it easy to update or extend functionality without impacting other parts of the system.

5. **Security**:
   - Modern authentication mechanisms (public key) protect against unauthorized access.

---

### **Potential Enhancements**

- **Error Queue**: Implement a durable queue (e.g., Azure Storage Queue) for failed uploads, enabling retries without losing data.
- **Advanced File Processing**: Add logic to handle custom processing rules or metadata checks before moving files.
- **Dynamic Timer**: Adjust the timer interval dynamically based on system load or the number of pending files in the SFTP folder.
- **IP whitelisting**: only trusted sources can connect to the SFTP server.