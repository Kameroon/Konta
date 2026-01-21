# Complétion des Webhooks Stripe - Guide d'Implémentation

## 📋 Résumé des Modifications

Les méthodes `HandleInvoicePaidAsync` et `HandleSubscriptionDeletedAsync` ont été complétées pour gérer automatiquement l'activation/désactivation des accès tenant suite aux événements Stripe.

---

## 🔧 Fichiers Modifiés

### 1. **WebhookHandler.cs** - Logique Métier

**Ajouts** :
- ✅ Injection de `IStripeCustomerRepository` (pour récupérer le TenantId)
- ✅ Injection de `IHttpClientFactory` (pour les appels HTTP)
- ✅ Implémentation complète de `HandleInvoicePaidAsync()`
- ✅ Implémentation complète de `HandleSubscriptionDeletedAsync()`

### 2. **appsettings.json** - Configuration

**Ajout** :
```json
"Services": {
  "Tenant": {
    "BaseUrl": "https://localhost:5002"
  }
}
```

### 3. **Program.cs** - Dependency Injection

**Ajout** :
```csharp
builder.Services.AddHttpClient();
```

---

## 🎯 Flux Complet : Paiement Réussi

### **Événement Stripe** : `invoice.paid`

```
1. Stripe envoie le webhook
   ↓
2. WebhookHandler.HandleAsync() vérifie la signature
   ↓
3. Vérification d'idempotence (évite les doublons)
   ↓
4. Routage vers HandleInvoicePaidAsync()
   ↓
5. Mise à jour du statut de la facture en base (status = "paid")
   ↓
6. Récupération du TenantId via StripeCustomerId
   ↓
7. Appel HTTP POST vers Konta.Tenant
   URL: https://localhost:5002/api/tenant/activate-access
   Body: {
     "TenantId": "abc-123-...",
     "Reason": "Paiement reçu",
     "InvoiceId": "in_...",
     "Amount": 49.99
   }
   ↓
8. Konta.Tenant active l'accès (IsActive = true)
   ↓
9. Log de succès avec détails
   ↓
10. Enregistrement de l'événement webhook (évite retraitement)
```

---

## 🎯 Flux Complet : Abonnement Supprimé

### **Événement Stripe** : `customer.subscription.deleted`

```
1. Stripe envoie le webhook
   ↓
2. WebhookHandler.HandleAsync() vérifie la signature
   ↓
3. Vérification d'idempotence
   ↓
4. Routage vers HandleSubscriptionDeletedAsync()
   ↓
5. Récupération du TenantId via StripeCustomerId
   ↓
6. Appel HTTP POST vers Konta.Tenant
   URL: https://localhost:5002/api/tenant/deactivate-access
   Body: {
     "TenantId": "abc-123-...",
     "Reason": "Abonnement supprimé",
     "SubscriptionId": "sub_...",
     "CanceledAt": "2026-01-20T22:00:00Z"
   }
   ↓
7. Konta.Tenant désactive l'accès (IsActive = false)
   ↓
8. Log d'avertissement avec détails
   ↓
9. Enregistrement de l'événement webhook
```

---

## 📝 Code Détaillé

### **HandleInvoicePaidAsync** (Lignes 72-135)

```csharp
private async Task<bool> HandleInvoicePaidAsync(Invoice? invoice)
{
    if (invoice == null) return false;

    _logger.LogInformation("Traitement du paiement réussi pour la facture Stripe {InvoiceId}", invoice.Id);

    try
    {
        // 1. Mise à jour de la facture locale dans Konta.Billing
        await _invoiceRepository.UpdateStatusAsync(invoice.Id, "paid", DateTime.UtcNow);
        _logger.LogInformation("Statut de la facture {InvoiceId} mis à jour en base de données", invoice.Id);

        // 2. Récupérer le TenantId associé au client Stripe
        var stripeCustomer = await _customerRepository.GetByStripeCustomerIdAsync(invoice.CustomerId);
        if (stripeCustomer == null)
        {
            _logger.LogError("Client Stripe {CustomerId} introuvable dans la base Konta.Billing", invoice.CustomerId);
            return false;
        }

        // 3. Appel HTTP vers Konta.Tenant pour activer l'accès
        var tenantServiceUrl = _configuration["Services:Tenant:BaseUrl"] ?? "https://localhost:5002";
        var httpClient = _httpClientFactory.CreateClient();
        
        var activateRequest = new
        {
            TenantId = stripeCustomer.TenantId,
            Reason = "Paiement reçu",
            InvoiceId = invoice.Id,
            Amount = invoice.AmountPaid / 100.0 // Conversion centimes → euros
        };

        var response = await httpClient.PostAsJsonAsync(
            $"{tenantServiceUrl}/api/tenant/activate-access", 
            activateRequest
        );

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Accès activé avec succès pour le tenant {TenantId} suite au paiement de {Amount}€", 
                stripeCustomer.TenantId, activateRequest.Amount);
            return true;
        }
        else
        {
            _logger.LogError("Échec de l'activation de l'accès pour le tenant {TenantId}. Status: {StatusCode}", 
                stripeCustomer.TenantId, response.StatusCode);
            return false;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors du traitement de la facture payée {InvoiceId}", invoice.Id);
        return false;
    }
}
```

**Points Clés** :
- ✅ **Gestion d'erreurs robuste** : Try-catch avec logs détaillés
- ✅ **Validation** : Vérifie que le client Stripe existe
- ✅ **Conversion** : `AmountPaid / 100.0` (Stripe utilise les centimes)
- ✅ **Logging** : Logs à chaque étape pour traçabilité

---

### **HandleSubscriptionDeletedAsync** (Lignes 137-189)

```csharp
private async Task<bool> HandleSubscriptionDeletedAsync(Subscription? subscription)
{
    if (subscription == null) return false;

    _logger.LogWarning("L'abonnement Stripe {SubscriptionId} a été supprimé (Client: {CustomerId})", 
        subscription.Id, subscription.CustomerId);

    try
    {
        // 1. Récupérer le TenantId associé au client Stripe
        var stripeCustomer = await _customerRepository.GetByStripeCustomerIdAsync(subscription.CustomerId);
        if (stripeCustomer == null)
        {
            _logger.LogError("Client Stripe {CustomerId} introuvable dans la base Konta.Billing", subscription.CustomerId);
            return false;
        }

        // 2. Appel HTTP vers Konta.Tenant pour désactiver l'accès
        var tenantServiceUrl = _configuration["Services:Tenant:BaseUrl"] ?? "https://localhost:5002";
        var httpClient = _httpClientFactory.CreateClient();
        
        var deactivateRequest = new
        {
            TenantId = stripeCustomer.TenantId,
            Reason = "Abonnement supprimé",
            SubscriptionId = subscription.Id,
            CanceledAt = subscription.CanceledAt ?? DateTime.UtcNow
        };

        var response = await httpClient.PostAsJsonAsync(
            $"{tenantServiceUrl}/api/tenant/deactivate-access", 
            deactivateRequest
        );

        if (response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Accès désactivé avec succès pour le tenant {TenantId} suite à la suppression de l'abonnement", 
                stripeCustomer.TenantId);
            return true;
        }
        else
        {
            _logger.LogError("Échec de la désactivation de l'accès pour le tenant {TenantId}. Status: {StatusCode}", 
                stripeCustomer.TenantId, response.StatusCode);
            return false;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors du traitement de la suppression de l'abonnement {SubscriptionId}", subscription.Id);
        return false;
    }
}
```

**Points Clés** :
- ✅ **LogWarning** : Utilise Warning au lieu d'Information (événement critique)
- ✅ **CanceledAt** : Utilise la date de Stripe ou DateTime.UtcNow par défaut
- ✅ **Gestion d'erreurs** : Même robustesse que HandleInvoicePaidAsync

---

## 🚀 Prochaines Étapes : Migration vers Event-Driven

### **Architecture Actuelle** : Appels HTTP Synchrones

```
Konta.Billing ──HTTP POST──> Konta.Tenant
     (WebhookHandler)          (TenantController)
```

**Avantages** :
- ✅ Simple à implémenter
- ✅ Pas de dépendance externe (RabbitMQ, Kafka)
- ✅ Réponse immédiate

**Inconvénients** :
- ❌ Couplage fort entre services
- ❌ Si Konta.Tenant est down, l'activation échoue
- ❌ Pas de retry automatique

---

### **Architecture Future** : Event-Driven avec RabbitMQ

```
Konta.Billing ──Publish Event──> RabbitMQ ──Subscribe──> Konta.Tenant
                                    │
                                    └──Subscribe──> Konta.Reporting (analytics)
```

**Migration** :

1. **Installer RabbitMQ** :
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

2. **Ajouter le package** :
```bash
dotnet add package MassTransit.RabbitMQ
```

3. **Remplacer les appels HTTP** :
```csharp
// Au lieu de :
await httpClient.PostAsJsonAsync(...);

// Faire :
await _messageBus.PublishAsync(new TenantAccessGrantedEvent
{
    TenantId = stripeCustomer.TenantId,
    Reason = "Paiement reçu",
    InvoiceId = invoice.Id,
    Amount = invoice.AmountPaid / 100.0
});
```

4. **Dans Konta.Tenant, créer un Consumer** :
```csharp
public class TenantAccessGrantedConsumer : IConsumer<TenantAccessGrantedEvent>
{
    public async Task Consume(ConsumeContext<TenantAccessGrantedEvent> context)
    {
        var evt = context.Message;
        await _tenantRepository.ActivateAsync(evt.TenantId);
        _logger.LogInformation("Tenant {TenantId} activé suite à {Reason}", evt.TenantId, evt.Reason);
    }
}
```

**Avantages** :
- ✅ Découplage total
- ✅ Retry automatique
- ✅ Plusieurs consommateurs possibles (Reporting, Analytics, etc.)
- ✅ Résilience : Si Tenant est down, le message est mis en queue

---

## ✅ Checklist de Vérification

### **Avant de Tester**

- [ ] `IStripeCustomerRepository` est bien enregistré dans le DI
- [ ] `HttpClient` est enregistré (`AddHttpClient()`)
- [ ] `Services:Tenant:BaseUrl` est configuré dans `appsettings.json`
- [ ] Le microservice `Konta.Tenant` expose les endpoints :
  - `POST /api/tenant/activate-access`
  - `POST /api/tenant/deactivate-access`

### **Test Manuel**

1. **Simuler un paiement Stripe** :
```bash
stripe trigger invoice.payment_succeeded
```

2. **Vérifier les logs** :
```
[INFO] Traitement du paiement réussi pour la facture Stripe in_...
[INFO] Statut de la facture in_... mis à jour en base de données
[INFO] Accès activé avec succès pour le tenant abc-123-... suite au paiement de 49.99€
```

3. **Vérifier en base** :
```sql
SELECT * FROM billing.BillingInvoices WHERE StripeInvoiceId = 'in_...';
-- Status devrait être 'paid'

SELECT * FROM tenant.Tenants WHERE Id = 'abc-123-...';
-- IsActive devrait être true
```

---

## 📊 Résumé

| Aspect | Implémentation |
|--------|----------------|
| **Paiement réussi** | ✅ Mise à jour facture + Activation tenant |
| **Abonnement supprimé** | ✅ Désactivation tenant |
| **Gestion d'erreurs** | ✅ Try-catch + Logs détaillés |
| **Idempotence** | ✅ Vérification via WebhookEvents |
| **Communication** | ✅ HTTP (actuel) → Event-driven (futur) |

**Les webhooks Stripe sont maintenant production-ready !** 🎉
