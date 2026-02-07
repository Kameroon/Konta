<template>
  <footer class="main-footer">
    <div class="footer-content">
      <div class="footer-grid">
        <div class="footer-brand">
          <div class="logo">
            <span class="logo-icon">K</span>
            <span class="logo-text">Konta<strong>ERP</strong></span>
          </div>
          <p class="brand-desc">
            La solution de gestion intelligente pour les entreprises modernes. 
            Automatisez votre comptabilité, gérez vos factures et boostez votre productivité.
          </p>
          <div class="social-links">
            <a href="#" class="social-link"><i class="fab fa-linkedin"></i></a>
            <a href="#" class="social-link"><i class="fab fa-twitter"></i></a>
            <a href="#" class="social-link"><i class="fab fa-facebook"></i></a>
          </div>
        </div>

        <div class="footer-links">
          <h4>Produit</h4>
          <ul>
            <li><router-link to="/plans">Tarifs</router-link></li>
            <li><router-link to="/plans">Fonctionnalités</router-link></li>
          </ul>
        </div>

        <div class="footer-links">
          <h4>Société</h4>
          <ul>
            <li><router-link to="/plans">À propos</router-link></li>
            <li><a href="#">Blog</a></li>
            <li><a href="#">Contact</a></li>
          </ul>
        </div>

        <div class="footer-newsletter">
          <h4>Restez informé</h4>
          <p>Recevez nos dernières mises à jour et articles.</p>
          <form @submit.prevent class="newsletter-form">
            <input type="email" placeholder="votre@email.com" required>
            <button type="submit">S'abonner</button>
          </form>
        </div>
      </div>

      <div class="footer-bottom">
        <p>&copy; 2026 Konta ERP. Tous droits réservés.</p>
        <div class="legal-links">
          <a href="#">Mentions Légales</a>
          <a href="#">Confidentialité</a>
          <a href="#">CGU</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped>
.main-footer {
  background: #0f172a;
  color: #94a3b8;
  padding: 20px 0 10px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  font-family: 'Inter', sans-serif;
}

.footer-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 40px;
}

.footer-grid {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 2fr;
  gap: 60px;
  margin-bottom: 30px;
}

.footer-brand .logo {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.logo-icon {
  background: #3b82f6;
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
}

.logo-text {
  font-size: 20px;
  color: white;
  letter-spacing: -0.5px;
}

.brand-desc {
  line-height: 1.6;
  margin-bottom: 16px;
  font-size: 0.95rem;
}

.social-links {
  display: flex;
  gap: 16px;
}

.social-link {
  color: #64748b;
  font-size: 1.2rem;
  transition: color 0.3s;
}

.social-link:hover {
  color: #3b82f6;
}

.footer-links h4, .footer-newsletter h4 {
  color: white;
  font-size: 1rem;
  font-weight: 700;
  margin-bottom: 16px;
}

.footer-links ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.footer-links li {
  margin-bottom: 8px;
}

.footer-links a {
  color: #94a3b8;
  text-decoration: none;
  transition: color 0.3s;
  font-size: 0.95rem;
}

.footer-links a:hover {
  color: white;
}

.footer-newsletter p {
  margin-bottom: 20px;
  font-size: 0.95rem;
}

.newsletter-form {
  display: flex;
  gap: 10px;
}

.newsletter-form input {
  flex: 1;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 12px 16px;
  color: white;
  outline: none;
}

.newsletter-form button {
  background: #3b82f6;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.3s;
}

.newsletter-form button:hover {
  opacity: 0.9;
}

.footer-bottom {
  padding-top: 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}

.legal-links {
  display: flex;
  gap: 24px;
}

.legal-links a {
  color: #64748b;
  text-decoration: none;
  transition: color 0.3s;
}

.legal-links a:hover {
  color: white;
}

@media (max-width: 1024px) {
  .footer-grid {
    grid-template-columns: 1fr 1fr;
    gap: 40px;
  }
}

@media (max-width: 640px) {
  .footer-grid {
    grid-template-columns: 1fr;
  }
  
  .footer-bottom {
    flex-direction: column;
    gap: 20px;
    text-align: center;
  }
  
  .legal-links {
    justify-content: center;
  }
}
</style>
