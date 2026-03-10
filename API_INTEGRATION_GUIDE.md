# 🚀 SyncVerse API - Integration Guide for Frontend Teams

## 📋 Table of Contents
1. [Quick Start](#quick-start)
2. [Authentication Flow](#authentication-flow)
3. [Common Patterns](#common-patterns)
4. [Framework-Specific Examples](#framework-specific-examples)
5. [Error Handling](#error-handling)
6. [Best Practices](#best-practices)

---

## 🎯 Quick Start

### Base URLs
```
Development: http://localhost:5131
Production: https://api.syncverse.com (TBD)
```

### Required Headers
```http
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

---

## 🔐 Authentication Flow

### 1. Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Ahmed Mohamed",
  "email": "ahmed@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "phoneNumber": "+201234567890",
  "role": "Manager"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Registration successful. Please check your email for OTP.",
  "data": {
    "userId": "user-id-here"
  }
}
```

### 2. Verify Email (OTP)
```http
POST /api/auth/verify-email/{userId}
Content-Type: application/json

{
  "otp": "123456"
}
```

### 3. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "ahmed@example.com",
  "password": "Password@123"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": "user-id",
    "email": "ahmed@example.com",
    "fullName": "Ahmed Mohamed",
    "role": "Manager",
    "expiresAt": "2024-01-16T10:30:00Z"
  }
}
```

---

## 📦 Common Patterns

### Standard Response Format

**Success Response:**
```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": { ... }
}
```

**Error Response:**
```json
{
  "isSuccess": false,
  "message": "Error message here",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

### Pagination (if implemented)
```json
{
  "isSuccess": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

---

## 💻 Framework-Specific Examples

### 🌐 React/Next.js (TypeScript)

#### Setup API Client
```typescript
// lib/api-client.ts
import axios, { AxiosInstance, AxiosError } from 'axios';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5131',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor - Add JWT token
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('jwt_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Response interceptor - Handle errors
    this.client.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Token expired - redirect to login
          localStorage.removeItem('jwt_token');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  async get<T>(url: string, params?: any): Promise<T> {
    const response = await this.client.get<T>(url, { params });
    return response.data;
  }

  async post<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.post<T>(url, data);
    return response.data;
  }

  async put<T>(url: string, data?: any): Promise<T> {
    const response = await this.client.put<T>(url, data);
    return response.data;
  }

  async delete<T>(url: string): Promise<T> {
    const response = await this.client.delete<T>(url);
    return response.data;
  }

  async uploadFile<T>(url: string, file: File, additionalData?: any): Promise<T> {
    const formData = new FormData();
    formData.append('file', file);
    
    if (additionalData) {
      Object.keys(additionalData).forEach(key => {
        formData.append(key, additionalData[key]);
      });
    }

    const response = await this.client.post<T>(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }
}

export const apiClient = new ApiClient();
```

#### Auth Service
```typescript
// services/auth.service.ts
import { apiClient } from '@/lib/api-client';

interface LoginRequest {
  email: string;
  password: string;
}

interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
  expiresAt: string;
}

export const authService = {
  login: async (credentials: LoginRequest) => {
    const response = await apiClient.post<{ data: AuthResponse }>(
      '/api/auth/login',
      credentials
    );
    
    if (response.data) {
      localStorage.setItem('jwt_token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  },

  getCurrentUser: () => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },
};
```

#### Projects Service
```typescript
// services/projects.service.ts
import { apiClient } from '@/lib/api-client';

export const projectsService = {
  getAll: async () => {
    return apiClient.get('/api/projects');
  },

  getById: async (id: string) => {
    return apiClient.get(`/api/projects/${id}`);
  },

  create: async (data: any) => {
    return apiClient.post('/api/projects', data);
  },

  update: async (id: string, data: any) => {
    return apiClient.put(`/api/projects/${id}`, data);
  },

  delete: async (id: string) => {
    return apiClient.delete(`/api/projects/${id}`);
  },

  uploadAttachment: async (projectId: string, file: File) => {
    return apiClient.uploadFile(
      '/api/project-attachments/upload',
      file,
      { projectId }
    );
  },
};
```

#### React Hook Example
```typescript
// hooks/useProjects.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsService } from '@/services/projects.service';

export const useProjects = () => {
  const queryClient = useQueryClient();

  const { data: projects, isLoading } = useQuery({
    queryKey: ['projects'],
    queryFn: projectsService.getAll,
  });

  const createProject = useMutation({
    mutationFn: projectsService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });

  return {
    projects,
    isLoading,
    createProject: createProject.mutate,
  };
};
```

---

### 📱 Flutter/Dart (Mobile)

#### Setup HTTP Client
```dart
// lib/core/api/api_client.dart
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiClient {
  static const String baseUrl = 'http://localhost:5131';
  late Dio _dio;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  ApiClient() {
    _dio = Dio(BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 30),
      receiveTimeout: const Duration(seconds: 30),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ));

    // Add token interceptor
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storage.read(key: 'jwt_token');
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            // Token expired - navigate to login
            await _storage.delete(key: 'jwt_token');
            // Navigate to login screen
          }
          return handler.next(error);
        },
      ),
    );
  }

  Future<T> get<T>(String path, {Map<String, dynamic>? queryParameters}) async {
    try {
      final response = await _dio.get(path, queryParameters: queryParameters);
      return response.data;
    } catch (e) {
      throw _handleError(e);
    }
  }

  Future<T> post<T>(String path, {dynamic data}) async {
    try {
      final response = await _dio.post(path, data: data);
      return response.data;
    } catch (e) {
      throw _handleError(e);
    }
  }

  Future<T> uploadFile<T>(
    String path,
    String filePath,
    Map<String, dynamic>? additionalData,
  ) async {
    try {
      FormData formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(filePath),
        ...?additionalData,
      });

      final response = await _dio.post(path, data: formData);
      return response.data;
    } catch (e) {
      throw _handleError(e);
    }
  }

  Exception _handleError(dynamic error) {
    if (error is DioException) {
      return Exception(error.response?.data['message'] ?? error.message);
    }
    return Exception('Unknown error occurred');
  }
}
```

#### Auth Repository
```dart
// lib/features/auth/data/repositories/auth_repository.dart
class AuthRepository {
  final ApiClient _apiClient;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  AuthRepository(this._apiClient);

  Future<AuthResponse> login(String email, String password) async {
    final response = await _apiClient.post(
      '/api/auth/login',
      data: {
        'email': email,
        'password': password,
      },
    );

    final authData = AuthResponse.fromJson(response['data']);
    await _storage.write(key: 'jwt_token', value: authData.token);
    return authData;
  }

  Future<void> logout() async {
    await _storage.delete(key: 'jwt_token');
  }
}
```

#### Projects Provider (with Riverpod)
```dart
// lib/features/projects/providers/projects_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';

final projectsProvider = FutureProvider<List<Project>>((ref) async {
  final apiClient = ref.watch(apiClientProvider);
  final response = await apiClient.get('/api/employee/projects');
  
  return (response['data'] as List)
      .map((json) => Project.fromJson(json))
      .toList();
});

final createProjectProvider = Provider((ref) {
  return (Project project) async {
    final apiClient = ref.watch(apiClientProvider);
    await apiClient.post('/api/projects', data: project.toJson());
    ref.invalidate(projectsProvider);
  };
});
```

---

### 🎮 Unity/C# (Gaming)

#### Setup HTTP Client
```csharp
// Scripts/Core/ApiClient.cs
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    private const string BaseUrl = "http://localhost:5131";
    private string _jwtToken;

    public static ApiClient Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetToken(string token)
    {
        _jwtToken = token;
        PlayerPrefs.SetString("jwt_token", token);
    }

    public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        string url = BaseUrl + endpoint;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            AddAuthHeader(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ApiResponse<T>>(request.downloadHandler.text);
                onSuccess?.Invoke(response.data);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError)
    {
        string url = BaseUrl + endpoint;
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            AddAuthHeader(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ApiResponse<T>>(request.downloadHandler.text);
                onSuccess?.Invoke(response.data);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    private void AddAuthHeader(UnityWebRequest request)
    {
        if (!string.IsNullOrEmpty(_jwtToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        }
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public bool isSuccess;
        public string message;
        public T data;
    }
}
```

#### Auth Manager
```csharp
// Scripts/Auth/AuthManager.cs
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Login(string email, string password)
    {
        var loginData = new LoginRequest
        {
            email = email,
            password = password
        };

        StartCoroutine(ApiClient.Instance.Post<AuthResponse>(
            "/api/auth/login",
            loginData,
            OnLoginSuccess,
            OnLoginError
        ));
    }

    void OnLoginSuccess(AuthResponse response)
    {
        ApiClient.Instance.SetToken(response.token);
        Debug.Log("Login successful!");
        
        // Load main scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    void OnLoginError(string error)
    {
        Debug.LogError($"Login failed: {error}");
        // Show error UI
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
        public string userId;
        public string fullName;
        public string role;
    }
}
```

---

## ⚠️ Error Handling

### HTTP Status Codes
```
200 OK - Success
400 Bad Request - Validation error
401 Unauthorized - Invalid/expired token
403 Forbidden - Insufficient permissions
404 Not Found - Resource not found
500 Internal Server Error - Server error
```

### Common Error Responses
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "Email is required",
    "Password must be at least 8 characters"
  ]
}
```

### Error Handling Best Practices
```typescript
try {
  const response = await apiClient.post('/api/projects', data);
  // Success handling
} catch (error) {
  if (error.response?.status === 400) {
    // Validation errors - show to user
    showValidationErrors(error.response.data.errors);
  } else if (error.response?.status === 401) {
    // Unauthorized - redirect to login
    redirectToLogin();
  } else {
    // Server error - show generic message
    showErrorToast('Something went wrong. Please try again.');
  }
}
```

---

## ✅ Best Practices

### 1. **Token Management**
- Store JWT securely (HttpOnly cookies in web, secure storage in mobile)
- Implement token refresh before expiration
- Clear token on logout

### 2. **API Calls**
- Use loading states in UI
- Implement request cancellation
- Add request timeouts
- Handle network errors gracefully

### 3. **Caching**
- Cache GET requests when appropriate
- Invalidate cache on mutations
- Use stale-while-revalidate pattern

### 4. **File Uploads**
- Show upload progress
- Validate file size/type before upload
- Handle upload cancellation

### 5. **Real-time Features**
- Reconnect SignalR on connection loss
- Queue messages while offline
- Sync data on reconnect

---

## 📞 Support & Resources

- **API Documentation:** http://localhost:5131/swagger
- **Postman Collection:** See `SyncVerse_API_Collection.postman_collection.json`
- **SignalR Guide:** See `SIGNALR_INTEGRATION_GUIDE.md`

---

**Last Updated:** March 2024
