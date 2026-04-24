/**
 * ALOud Checkout System
 * JavaScript orchestrator for progressive single-page checkout
 */

class CheckoutManager {
    constructor() {
        this.currentStep = 1;
        this.maxStep = 4;
        this.checkoutData = {
            address: null,
            delivery: null,
            payment: null,
            sessionId: null
        };
        this.isAuthenticated = false;
        this.userAddresses = [];
        this.cartItems = [];
        this.totals = { subtotal: 0, shipping: 0, tax: 0, total: 0 };
        this.startTime = Date.now();
        this.stepStartTime = Date.now();
        this.init();
    }

    async init() {
        console.log('Initializing ALOud Checkout System...');
        await this.checkAuthenticationStatus();
        if (this.isAuthenticated) {
            await this.loadUserAddresses();
        }
        await this.loadCartData();
        this.initializeUI();
        this.bindEventHandlers();
        console.log('Checkout system initialized successfully');
    }

    /**
     * Check if user is authenticated and update the UI accordingly
     */
    async checkAuthenticationStatus() {
        const statusIcon = document.getElementById('auth-status-icon');
        const statusText = document.getElementById('auth-status-text');
        const statusSubtitle = document.getElementById('auth-status-subtitle');
        const authActions = document.getElementById('auth-actions');

        if (!statusIcon || !statusText) return;

        let userData = null;
        try {
            const response = await fetch('/api/v1/account/profile', {
                method: 'GET',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' }
            });
            if (response.ok) {
                const result = await response.json();
                userData = result.data || result;
                this.isAuthenticated = true;
            }
        } catch (error) {
            console.warn('Auth check failed:', error);
        }

        if (this.isAuthenticated && userData) {
            statusIcon.innerHTML = `
                <div class="d-flex align-items-center justify-content-center rounded-circle"
                     style="width: 40px; height: 40px; background-color: var(--color-accent); color: white;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                    </svg>
                </div>`;
            statusText.textContent = `Welcome back, ${userData.firstName || userData.email}!`;
            statusSubtitle.textContent = 'Using saved account information';
            authActions.innerHTML = `
                <button type="button" class="btn btn-outline-secondary btn-sm" onclick="checkoutManager.logout()">
                    Sign Out
                </button>`;
            const savedSection = document.getElementById('saved-addresses-section');
            if (savedSection) savedSection.style.display = 'block';
            const saveCheck = document.getElementById('saveAddress');
            if (saveCheck) saveCheck.closest('.form-check').style.display = 'block';
        } else {
            statusIcon.innerHTML = `
                <div class="d-flex align-items-center justify-content-center rounded-circle"
                     style="width: 40px; height: 40px; background-color: var(--color-text-secondary); color: white;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                    </svg>
                </div>`;
            statusText.textContent = 'Checkout as Guest';
            statusSubtitle.textContent = 'Quick and easy - no account required';
            authActions.innerHTML = `
                <button type="button" class="btn btn-primary btn-sm" onclick="checkoutManager.showLoginPrompt()">
                    Sign In
                </button>`;
            const savedSection = document.getElementById('saved-addresses-section');
            if (savedSection) savedSection.style.display = 'none';
            const saveCheck = document.getElementById('saveAddress');
            if (saveCheck) saveCheck.closest('.form-check').style.display = 'none';
        }
    }

    async loadUserAddresses() {
        if (!this.isAuthenticated) return;
        try {
            const response = await fetch('/api/v1/addresses', {
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' }
            });
            if (response.ok) {
                const result = await response.json();
                this.userAddresses = result.data || result || [];
                this.renderSavedAddresses();
            }
        } catch (error) {
            console.warn('Could not load user addresses:', error);
        }
    }

    renderSavedAddresses() {
        const container = document.getElementById('saved-addresses-list');
        if (!container || !this.userAddresses.length) {
            const section = document.getElementById('saved-addresses-section');
            if (section) section.style.display = 'none';
            return;
        }

        container.innerHTML = this.userAddresses.map(address => `
            <div class="col-md-6">
                <div class="saved-address-card" data-address-id="${address.id}">
                    <div class="p-3 rounded border">
                        <div class="form-check mb-0">
                            <input class="form-check-input" type="radio" name="saved-address" value="${address.id}" id="address-${address.id}">
                            <label class="form-check-label w-100" for="address-${address.id}">
                                <div class="fw-semibold">${address.firstName} ${address.lastName}</div>
                                <div class="small text-muted">
                                    ${address.addressLine1 || address.street || ''}<br>
                                    ${address.city}, ${address.postalCode}<br>
                                    ${address.country}
                                </div>
                                ${address.isDefault ? '<span class="badge bg-primary">Default</span>' : ''}
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');

        container.querySelectorAll('input[name="saved-address"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                if (e.target.checked) this.selectSavedAddress(e.target.value);
            });
        });
        document.getElementById('saved-addresses-section').style.display = 'block';
    }

    selectSavedAddress(addressId) {
        const address = this.userAddresses.find(addr => addr.id === addressId);
        if (!address) return;

        document.getElementById('firstName').value = address.firstName || '';
        document.getElementById('lastName').value = address.lastName || '';
        document.getElementById('street').value = address.addressLine1 || address.street || '';
        document.getElementById('city').value = address.city || '';
        document.getElementById('postalCode').value = address.postalCode || '';
        document.getElementById('country').value = address.country || '';
        document.getElementById('phoneNumber').value = address.phoneNumber || '';

        document.getElementById('address-form-title').textContent = 'Selected Address';
        document.getElementById('add-new-address-btn').style.display = 'inline-block';

        this.checkoutData.address = { ...address, isSaved: true };
        this.validateAddressForm();
    }

    async loadCartData() {
        try {
            const response = await fetch('/api/v1/cart', { credentials: 'include' });
            if (response.ok) {
                const result = await response.json();
                this.cartItems = result.data || result || [];
                this.calculateTotals();
            }
        } catch (error) {
            console.error('Could not load cart data:', error);
            this.showError('Unable to load cart data. Please refresh and try again.');
        }
    }

    calculateTotals() {
        this.totals.subtotal = this.cartItems.reduce((sum, item) => sum + (item.total || item.lineTotal || 0), 0);
        this.totals.shipping = 0;
        this.totals.tax = this.totals.subtotal * 0.08;
        this.totals.total = this.totals.subtotal + this.totals.shipping + this.totals.tax;
        this.updateOrderSummary();
    }

    updateOrderSummary() {
        const el = (id) => document.getElementById(id);
        if (el('summary-subtotal')) el('summary-subtotal').textContent = `${this.totals.subtotal.toLocaleString()} MAD`;
        if (el('summary-tax')) el('summary-tax').textContent = `${Math.round(this.totals.tax).toLocaleString()} MAD`;
        if (el('summary-total')) el('summary-total').textContent = `${Math.round(this.totals.total).toLocaleString()} MAD`;
    }

    initializeUI() {
        this.showStep(1);
        this.initFormValidation();
        this.updateProgressIndicator();
    }

    bindEventHandlers() {
        const on = (id, event, fn) => document.getElementById(id)?.addEventListener(event, fn);

        on('continue-to-delivery', 'click', () => this.proceedToStep(2));
        on('continue-to-payment', 'click', () => this.proceedToStep(3));
        on('continue-to-review', 'click', () => this.proceedToStep(4));
        on('back-to-shipping', 'click', () => this.goToStep(1));
        on('back-to-delivery', 'click', () => this.goToStep(2));
        on('back-to-payment', 'click', () => this.goToStep(3));
        on('complete-order', 'click', () => this.completeOrder());
        on('address-form', 'input', () => this.validateAddressForm());
        on('add-new-address-btn', 'click', () => this.showNewAddressForm());

        document.querySelectorAll('input[name="payment"]').forEach(radio => {
            radio.addEventListener('change', (e) => this.selectPaymentMethod(e.target.value));
        });
        document.querySelectorAll('input[name="delivery"]').forEach(radio => {
            radio.addEventListener('change', (e) => this.selectDeliveryMethod(e.target.value));
        });
        document.querySelectorAll('[data-edit-step]').forEach(btn => {
            btn.addEventListener('click', (e) => this.goToStep(parseInt(e.target.dataset.editStep)));
        });

        // Default delivery selection
        this.selectDeliveryMethod('standard');
    }

    initFormValidation() {
        const form = document.getElementById('address-form');
        if (!form) return;
        form.querySelectorAll('input, select').forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => {
                if (input.classList.contains('is-invalid')) this.validateField(input);
            });
        });
    }

    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';
        field.classList.remove('is-valid', 'is-invalid');

        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required';
        }
        if (field.id === 'phoneNumber' && value && !/^[\+]?[0-9\s\-\(\)]{8,}$/.test(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid phone number';
        }
        if (field.id === 'postalCode' && value && !/^[0-9A-Za-z\s\-]{3,}$/.test(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid postal code';
        }

        field.classList.add(isValid ? 'is-valid' : 'is-invalid');
        const feedback = field.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = errorMessage;
        }
        return isValid;
    }

    validateAddressForm() {
        const form = document.getElementById('address-form');
        if (!form) return false;
        let isValid = true;
        form.querySelectorAll('input[required], select[required]').forEach(input => {
            if (!this.validateField(input)) isValid = false;
        });
        const btn = document.getElementById('continue-to-delivery');
        if (btn) btn.disabled = !isValid;
        return isValid;
    }

    showNewAddressForm() {
        document.querySelectorAll('input[name="saved-address"]').forEach(r => r.checked = false);
        document.getElementById('address-form').reset();
        document.getElementById('address-form-title').textContent = 'New Shipping Address';
        document.getElementById('add-new-address-btn').style.display = 'none';
        this.checkoutData.address = null;
        this.validateAddressForm();
    }

    selectDeliveryMethod(method) {
        this.checkoutData.delivery = { method };
        document.querySelectorAll('.delivery-option').forEach(o => o.classList.remove('active'));
        const sel = document.querySelector(`[data-method="${method}"]`);
        if (sel) sel.classList.add('active');
    }

    selectPaymentMethod(method) {
        this.checkoutData.payment = { method };
        const container = document.getElementById('payment-details-container');
        if (!container) return;

        if (method === 'stripe') {
            container.innerHTML = `
                <div class="payment-form mb-4">
                    <h6 class="mb-3">Credit Card Information</h6>
                    <div class="p-3 rounded text-center text-muted" style="background: var(--color-background); border: 1px solid var(--color-border);">
                        <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor" class="mb-3">
                            <path d="M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z"/>
                        </svg>
                        <p class="mb-0">Secure credit card processing</p>
                        <small>Payment processing will be completed on confirmation</small>
                    </div>
                </div>`;
        } else if (method === 'paypal') {
            container.innerHTML = `
                <div class="payment-form mb-4">
                    <h6 class="mb-3">PayPal Payment</h6>
                    <div class="p-3 rounded text-center text-muted" style="background: var(--color-background); border: 1px solid var(--color-border);">
                        <p class="mb-0">You'll be redirected to PayPal</p>
                        <small>Complete your payment securely with PayPal</small>
                    </div>
                </div>`;
        } else {
            container.innerHTML = '';
        }
    }

    goToStep(stepNumber) {
        if (stepNumber < 1 || stepNumber > this.maxStep) return;
        this.currentStep = stepNumber;
        this.stepStartTime = Date.now();
        this.showStep(stepNumber);
        this.updateProgressIndicator();
        if (stepNumber === 4) this.updateReviewSection();
    }

    async proceedToStep(stepNumber) {
        if (!await this.validateCurrentStep()) return;
        await this.captureStepData();

        // If moving to Review step, sync all data to server first
        if (stepNumber === 4) {
            const success = await this.syncCheckoutData();
            if (!success) return;
        }

        this.goToStep(stepNumber);
    }

    async validateCurrentStep() {
        switch (this.currentStep) {
            case 1:
                if (!this.validateAddressForm()) {
                    this.showError('Please complete all required address fields');
                    return false;
                }
                return true;
            case 2:
                if (!this.checkoutData.delivery) {
                    this.showError('Please select a delivery method');
                    return false;
                }
                return true;
            case 3:
                if (!this.checkoutData.payment) {
                    this.showError('Please select a payment method');
                    return false;
                }
                return true;
            case 4:
                const termsCheck = document.getElementById('terms-acceptance');
                if (!termsCheck?.checked) {
                    if (termsCheck) termsCheck.classList.add('is-invalid');
                    this.showError('Please accept the terms and conditions');
                    return false;
                }
                return true;
            default:
                return true;
        }
    }

    async captureStepData() {
        if (this.currentStep === 1) {
            const form = document.getElementById('address-form');
            const fd = new FormData(form);
            this.checkoutData.address = {
                firstName: fd.get('firstName'),
                lastName: fd.get('lastName'),
                addressLine1: fd.get('street'),
                city: fd.get('city'),
                state: fd.get('city'), // fallback
                postalCode: fd.get('postalCode'),
                country: fd.get('country'),
                phoneNumber: fd.get('phoneNumber'),
                saveAddress: fd.get('saveAddress') === 'on'
            };
        }
    }

    showStep(stepNumber) {
        document.querySelectorAll('.checkout-step').forEach(s => s.style.display = 'none');
        const target = document.getElementById(`step-${stepNumber}`);
        if (target) target.style.display = 'block';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    updateProgressIndicator() {
        document.querySelectorAll('.step').forEach((step, index) => {
            const num = index + 1;
            step.classList.remove('active', 'completed');
            if (num < this.currentStep) step.classList.add('completed');
            else if (num === this.currentStep) step.classList.add('active');
        });
    }

    updateReviewSection() {
        if (this.checkoutData.address) {
            const a = this.checkoutData.address;
            const el = document.getElementById('review-shipping-address');
            if (el) el.innerHTML = `${a.firstName} ${a.lastName}<br>${a.addressLine1}<br>${a.city}, ${a.postalCode}<br>${a.country}<br>${a.phoneNumber || ''}`;
        }
        if (this.checkoutData.delivery) {
            const el = document.getElementById('review-delivery-method');
            if (el) el.textContent = this.checkoutData.delivery.method === 'standard' ? 'Standard Delivery (3-5 business days) - Free' : this.checkoutData.delivery.method;
        }
        if (this.checkoutData.payment) {
            const el = document.getElementById('review-payment-method');
            if (el) el.textContent = this.checkoutData.payment.method === 'stripe' ? 'Credit Card' : this.checkoutData.payment.method === 'paypal' ? 'PayPal' : this.checkoutData.payment.method;
        }
    }

    /**
     * Synchronizes local checkout data with the server
     */
    async syncCheckoutData() {
        const button = document.getElementById('continue-to-review');
        if (button) {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Preparing Review...';
        }

        try {
            // Helper to throw on bad responses
            const checkRes = async (res, stepName) => {
                if (!res.ok) {
                    const err = await res.json().catch(() => ({}));
                    throw new Error(err.message || err.error || err.title || `Failed to sync ${stepName} (${res.status})`);
                }
                return res;
            };

            // Step 1: Start/Get checkout session
            const startRes = await fetch('/api/v1/checkout/start', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    email: this.isAuthenticated ? null : (this.checkoutData.address?.email || 'guest@aloud.ma'),
                    isGuestCheckout: !this.isAuthenticated
                })
            });

            await checkRes(startRes, 'checkout session');
            const startResult = await startRes.json();
            const checkoutId = startResult.data?.id || startResult.id;
            this.checkoutData.sessionId = checkoutId;

            // Step 2: Set shipping address
            const addr = this.checkoutData.address;
            const shipRes = await fetch(`/api/v1/checkout/${checkoutId}/shipping-address`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    firstName: addr.firstName,
                    lastName: addr.lastName,
                    addressLine1: addr.addressLine1,
                    city: addr.city,
                    state: addr.state || addr.city,
                    postalCode: addr.postalCode,
                    country: addr.country || 'MA', // Fallback for safety
                    phoneNumber: addr.phoneNumber
                })
            });
            await checkRes(shipRes, 'shipping address');

            // Step 3: Copy shipping to billing
            const billRes = await fetch(`/api/v1/checkout/${checkoutId}/copy-shipping-to-billing`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });
            await checkRes(billRes, 'billing address');

            // Step 4: Set shipping method
            const shippingMethod = this.checkoutData.delivery?.method || 'standard';
            const methodRes = await fetch(`/api/v1/checkout/${checkoutId}/shipping-method`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(shippingMethod)
            });
            await checkRes(methodRes, 'shipping method');

            // Step 5: Set payment method
            const paymentMethod = this.checkoutData.payment?.method || 'stripe';
            const payRes = await fetch(`/api/v1/checkout/${checkoutId}/payment-method`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ paymentMethod: paymentMethod })
            });
            await checkRes(payRes, 'payment method');

            return true;
        } catch (error) {
            console.error('Checkout sync error:', error);
            this.showError(error.message || 'Unable to sync checkout data. Please try again.');
            return false;
        } finally {
            if (button) {
                button.disabled = false;
                button.textContent = 'Continue to Review';
            }
        }
    }

    async completeOrder() {
        const button = document.getElementById('complete-order');
        const buttonText = button?.querySelector('.button-text');
        const buttonSpinner = button?.querySelector('.button-spinner');

        if (!await this.validateCurrentStep()) return;

        // Show loading state
        if (button) button.disabled = true;
        if (buttonText) buttonText.textContent = 'Processing...';
        if (buttonSpinner) buttonSpinner.style.display = 'inline-block';

        try {
            const checkoutId = this.checkoutData.sessionId;
            if (!checkoutId) throw new Error('No checkout session found. Please go back and try again.');

            // Complete checkout
            const completeRes = await fetch(`/api/v1/checkout/${checkoutId}/complete`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({
                    checkoutId: checkoutId,
                    paymentMethod: this.checkoutData.payment?.method || 'stripe',
                    saveAddressForFuture: this.checkoutData.address?.saveAddress || false
                })
            });

            if (!completeRes.ok) {
                const err = await completeRes.json().catch(() => ({}));
                throw new Error(err.message || `Failed to complete checkout (${completeRes.status})`);
            }

            // Redirect to success page
            window.location.href = `/Checkout/Success/${checkoutId}`;

        } catch (error) {
            console.error('Checkout completion error:', error);
            this.showError(error.message || 'Unable to complete your order. Please try again.');
            if (button) button.disabled = false;
            if (buttonText) buttonText.textContent = 'Complete Order';
            if (buttonSpinner) buttonSpinner.style.display = 'none';
        }
    }

    showLoginPrompt() {
        if (confirm('Sign in to save addresses and view order history. Continue to login page?')) {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }
    }

    async logout() {
        try {
            await fetch('/api/v1/account/logout', { method: 'POST', credentials: 'include' });
            window.location.reload();
        } catch (error) {
            console.error('Logout error:', error);
        }
    }

    showError(message) {
        let errorAlert = document.querySelector('.checkout-error-alert');
        if (!errorAlert) {
            errorAlert = document.createElement('div');
            errorAlert.className = 'alert alert-danger checkout-error-alert';
            errorAlert.innerHTML = `
                <div class="d-flex align-items-center">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" class="me-2">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
                    </svg>
                    <span class="error-message"></span>
                </div>`;
            const currentStepEl = document.getElementById(`step-${this.currentStep}`);
            if (currentStepEl) currentStepEl.insertBefore(errorAlert, currentStepEl.firstChild);
        }
        const msgEl = errorAlert.querySelector('.error-message');
        if (msgEl) msgEl.textContent = message;
        errorAlert.scrollIntoView({ behavior: 'smooth', block: 'center' });
        setTimeout(() => { if (errorAlert.parentNode) errorAlert.remove(); }, 6000);
    }
}

// Initialize checkout system when DOM is loaded
let checkoutManager;
document.addEventListener('DOMContentLoaded', function () {
    checkoutManager = new CheckoutManager();
});
window.checkoutManager = checkoutManager;