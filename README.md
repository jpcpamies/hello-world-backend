# 👋 Hello World API (.NET 8)

Este es un proyecto de backend desarrollado en **.NET 8 Web API** que expone un endpoint de resumen de texto con integración a **OpenAI**. Incluye validación, logs estructurados, documentación Swagger y despliegue automático a **Azure App Service** mediante **GitHub Actions**.

---

## 🚀 Endpoints principales

| Método | Ruta                            | Descripción                        |
|--------|----------------------------------|------------------------------------|
| `GET`  | `/api/v1/health`                 | Health check de la API             |
| `POST` | `/api/v1/text/summarize`         | Resumen de texto con OpenAI        |
| `GET`  | `/swagger`                       | Documentación interactiva          |

---

## 📦 Estructura del proyecto

```bash
HelloWorldBackend/
├── Controllers/
├── Models/
├── Services/
├── Configuration/
├── Middleware/
├── appsettings.json
├── Program.cs
└── .github/workflows/
    └── develop.yml
```

---

## ⚙️ Stack técnico

- **.NET 8 Web API**
- **OpenAI GPT (API Key por entorno)**
- **FluentValidation**
- **Serilog para logs**
- **Swagger/OpenAPI**
- **Rate limiting**
- **CORS & security headers**
- **CI/CD con GitHub Actions**
- **Despliegue en Azure App Service**

---

## 🔐 Secrets requeridos (GitHub Actions)

| Nombre del secret                     | Descripción                         |
|--------------------------------------|-------------------------------------|
| `AZURE_WEBAPP_NAME_DEV`              | Nombre de la app en Azure (ej. `hello-world-api-dev`) |
| `AZURE_WEBAPP_PUBLISH_PROFILE_DEV`   | XML del archivo `.PublishSettings` exportado desde Azure Portal |

---

## 📦 Despliegue automático

Cada vez que se hace push a la rama `main`, el workflow `develop.yml`:

1. Restaura dependencias
2. Compila la app
3. Ejecuta los tests
4. Publica los artefactos
5. Despliega a Azure App Service `hello-world-api-dev`

Puedes monitorizar el proceso desde la pestaña **Actions** en GitHub.

---

## ✅ Health Check

Puedes validar que la API está corriendo en Azure accediendo a:

```
https://hello-world-api-dev.azurewebsites.net/api/v1/health
```

---

## ✉️ Request ejemplo para resumen

```bash
curl -X POST https://hello-world-api-dev.azurewebsites.net/api/v1/text/summarize \
  -H "Content-Type: application/json" \
  -d '{
    "text": "La inteligencia artificial está revolucionando la forma en que trabajamos. Permite automatizar procesos, mejorar la productividad y tomar mejores decisiones.",
    "maxSummaryLength": 150
}'
```

---

## 📌 Notas

- El API Key de OpenAI debe ir configurado en el entorno como variable de entorno `OPENAI_API_KEY`
- Puedes personalizar los límites de resumen, tokens y formato de salida en la clase `OpenAIService`

---

## 📍 Roadmap futuro

- [ ] Integración con Application Insights
- [ ] Despliegue a producción (`hello-world-api-prod`)
- [ ] Mejora de logging y trazabilidad
- [ ] Test automatizados para endpoint `/summarize`

---

## 👨‍💻 Autor

Desarrollado por Jordi Pamies  
[https://jordipamies.com](https://jordipamies.com)
