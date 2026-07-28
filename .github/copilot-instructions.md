# Copilot Instructions

## Project Guidelines
- User wants CQRS + MediatR + FluentValidation approach (not Service pattern). Application and Presentation layers only. Never modify Domain/Infrastructure. Use the exact order: Use Case Overview → Application Design → Command/Query → Validator → DTOs → Handler → API Endpoint → Sample Request → Sample Response → Unit Test Suggestions → Integration Test Suggestions → Architecture Review. Handlers are orchestrators only — load aggregate, call aggregate methods, persist, return result. Controllers are thin: Mediator.Send() only.