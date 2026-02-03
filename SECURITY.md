# GuestFlow Security Policy

This policy defines the security posture, vulnerability reporting protocol, and maintenance standards for the GuestFlow platform.

---

## 🛡 Supported Versions

Security updates are provided for the following versions:

| Version | Status |
| :--- | :--- |
| **1.x** | ✅ Stable / Active |
| **< 1.0** | ❌ End of Life |

---

## 🔍 Vulnerability Disclosure Protocol

Ensuring the security of guest PII (Personally Identifiable Information) is our highest priority. We welcome responsible disclosure from security researchers.

### Reporting a Vulnerability

- **Primary Channel**: Use the [GitHub Security Advisory](https://github.com/furkancengiz6/GuestFlow/security/advisories/new) tool.
- **Direct Contact**: For urgent internal matters, contact `security@guestflow.com`.

### Submission Requirements

- **Impact Assessment**: Description of the risk (e.g., Auth Bypass, Data Leakage).
- **Proof of Concept (PoC)**: Minimal steps to reproduce the issue.
- **Component Context**: Specific API endpoint or service layer affected.

### Response SLA

- **Initial Acknowledgement**: < 48 Hours.
- **Triage & Severity Assignment**: < 7 Business Days.
- **Resolution Path**: Dependent on severity; critical issues are prioritized for immediate hotfix.

---

## 🔒 Platform Hardening Standards

GuestFlow follows a "Security-by-Design" philosophy:

1. **Identity Control**: Stateless JWT authentication combined with Role-Based Access Control (RBAC).
2. **Input Integrity**: All user-provided data is sanitized via `Ganss.XSS` and validated against strict FluentValidation rules.
3. **Data at Rest**: Sensitive fields (Passport IDs, Personal Contacts) are encrypted using AES-256.
4. **Audit Integrity**: Immutable logging of all administrative actions and PII access via Serilog.
5. **Infrastructure Hygiene**: Automated dependency scanning to detect and mitigate CVEs in third-party libraries.

---

## ⚠️ Out of Scope

- Theoretical vulnerabilities without a working PoC.
- Social engineering or physical security attacks.
- Failures due to local development environment overrides (e.g., intentional CORS wildcarding in local dev configs).
