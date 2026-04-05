/**
 * ALOud Checkout System
 * Comprehensive JavaScript orchestrator for progressive single-page checkout
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
        this.totals = {
            subtotal: 0,
            shipping: 0,
            tax: 0,
            total: 0
        };
        
        // Analytics tracking
        this.analytics = new CheckoutAnalytics();
        this.startTime = Date.now();

        this.init();
    }

    /**
     * Initialize the checkout system
     */
    async init() {
        console.log('Initializing ALOud Checkout System...');
        
        // Track checkout start
        this.analytics.trackEvent('checkout_started', {
            timestamp: new Date().toISOString(),
            user_type: 'unknown' // Will be updated after auth check
        });
        
        // Check authentication status
        await this.checkAuthenticationStatus();
        
        // Load user data if authenticated
        if (this.isAuthenticated) {
            await this.loadUserAddresses();
        }
        
        // Initialize cart data
        await this.loadCartData();
        
        // Setup UI
        this.initializeUI();
        this.bindEventHandlers();
        
        // Start stock validation
        this.startStockValidation();
        
        console.log('Checkout system initialized successfully');
    }

    /**
     * Check if user is authenticated
     */
    async checkAuthenticationStatus() {
        try {
            const response = await fetch('/api/v1/account/profile', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            this.isAuthenticated = response.ok;
            
            this.analytics.trackEvent('auth_status_checked', {
                user_type: this.isAuthenticated ? 'authenticated' : 'guest',
                has_saved_addresses: this.isAuthenticated && userData ? true : false
            });
            
            if (this.isAuthenticated && userData) {
            // Authenticated user
            statusIcon.innerHTML = `
                <div class="d-flex align-items-center justify-content-center rounded-circle" 
                     style="width: 40px; height: 40px; background-color: var(--color-accent); color: white;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                    </svg>
                </div>
            `;
            statusText.textContent = `Welcome back, ${userData.firstName || userData.email}!`;
            statusSubtitle.textContent = 'Using saved account information';
            authActions.innerHTML = `
                <button type="button" class="btn btn-outline-secondary btn-sm" onclick="checkoutManager.logout()">
                    Sign Out
                </button>
            `;
            
            // Show saved addresses section
            document.getElementById('saved-addresses-section').style.display = 'block';
            document.getElementById('saveAddress').closest('.form-check').style.display = 'block';
        } else {
            // Guest user
            statusIcon.innerHTML = `
                <div class="d-flex align-items-center justify-content-center rounded-circle" 
                     style="width: 40px; height: 40px; background-color: var(--color-text-secondary); color: white;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                    </svg>
                </div>
            `;
            statusText.textContent = 'Checkout as Guest';
            statusSubtitle.textContent = 'Quick and easy - no account required';
            authActions.innerHTML = `
                <button type="button" class="btn btn-primary btn-sm" onclick="checkoutManager.showLoginPrompt()">
                    Sign In
                </button>
            `;
            
            // Hide saved addresses section and save option
            document.getElementById('saved-addresses-section').style.display = 'none';
            document.getElementById('saveAddress').closest('.form-check').style.display = 'none';
        }
    }

    /**
     * Load user saved addresses
     */
    async loadUserAddresses() {
        if (!this.isAuthenticated) return;

        try {
            const response = await fetch('/api/v1/addresses', {
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                this.userAddresses = result.data || [];
                this.renderSavedAddresses();
            }
        } catch (error) {
            console.warn('Could not load user addresses:', error);
        }
    }

    /**
     * Render saved addresses in the UI
     */
    renderSavedAddresses() {
        const container = document.getElementById('saved-addresses-list');
        
        if (!this.userAddresses.length) {
            document.getElementById('saved-addresses-section').style.display = 'none';
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
                                    ${address.street}<br>
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

        // Add event listeners for saved address selection
        container.querySelectorAll('input[name="saved-address"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.selectSavedAddress(e.target.value);
                }
            });
        });

        document.getElementById('saved-addresses-section').style.display = 'block';
    }

    /**
     * Select a saved address and populate the form
     */
    selectSavedAddress(addressId) {
        const address = this.userAddresses.find(addr => addr.id === addressId);
        if (!address) return;

        // Populate form fields
        document.getElementById('firstName').value = address.firstName;
        document.getElementById('lastName').value = address.lastName;
        document.getElementById('street').value = address.street;
        document.getElementById('city').value = address.city;
        document.getElementById('postalCode').value = address.postalCode;
        document.getElementById('country').value = address.country;
        document.getElementById('phoneNumber').value = address.phoneNumber || '';

        // Update form title and show "Add New" button
        document.getElementById('address-form-title').textContent = 'Selected Address';
        document.getElementById('add-new-address-btn').style.display = 'inline-block';
        
        // Mark form as using saved address
        this.checkoutData.address = { ...address, isSaved: true };
        
        this.validateAddressForm();
    }

    /**
     * Load cart data for checkout
     */
    async loadCartData() {
        try {
            const response = await fetch('/api/v1/cart', {
                credentials: 'include'
            });

            if (response.ok) {
                const result = await response.json();
                this.cartItems = result.data || [];
                this.calculateTotals();
            }
        } catch (error) {
            console.error('Could not load cart data:', error);
            this.showError('Unable to load cart data. Please refresh and try again.');
        }
    }

    /**
     * Calculate order totals
     */
    calculateTotals() {
        this.totals.subtotal = this.cartItems.reduce((sum, item) => sum + item.total, 0);
        this.totals.shipping = 0; // Free shipping
        this.totals.tax = this.totals.subtotal * 0.08; // 8% tax
        this.totals.total = this.totals.subtotal + this.totals.shipping + this.totals.tax;
        
        this.updateOrderSummary();
    }

    /**
     * Update order summary display
     */
    updateOrderSummary() {
        document.getElementById('summary-subtotal').textContent = `${this.totals.subtotal.toLocaleString()} MAD`;
        document.getElementById('summary-tax').textContent = `${Math.round(this.totals.tax).toLocaleString()} MAD`;
        document.getElementById('summary-total').textContent = `${Math.round(this.totals.total).toLocaleString()} MAD`;
    }

    /**
     * Initialize UI elements
     */
    initializeUI() {
        // Set initial step
        this.stepStartTime = Date.now();
        this.showStep(1);
        
        // Initialize form validation
        this.initFormValidation();
        
        // Setup step navigation
        this.updateProgressIndicator();
    }

    /**
     * Bind all event handlers
     */
    bindEventHandlers() {
        // Step navigation buttons
        document.getElementById('continue-to-delivery')?.addEventListener('click', () => this.proceedToStep(2));
        document.getElementById('continue-to-payment')?.addEventListener('click', () => this.proceedToStep(3));
        document.getElementById('continue-to-review')?.addEventListener('click', () => this.proceedToStep(4));
        
        document.getElementById('back-to-shipping')?.addEventListener('click', () => this.goToStep(1));
        document.getElementById('back-to-delivery')?.addEventListener('click', () => this.goToStep(2));
        document.getElementById('back-to-payment')?.addEventListener('click', () => this.goToStep(3));
        
        // Complete order button
        document.getElementById('complete-order')?.addEventListener('click', () => this.completeOrder());
        
        // Address form changes
        document.getElementById('address-form')?.addEventListener('input', () => this.validateAddressForm());
        
        // Add new address button
        document.getElementById('add-new-address-btn')?.addEventListener('click', () => this.showNewAddressForm());
        
        // Payment method selection
        document.querySelectorAll('input[name="payment"]').forEach(radio => {
            radio.addEventListener('change', (e) => this.selectPaymentMethod(e.target.value));
        });
        
        // Delivery method selection
        document.querySelectorAll('input[name="delivery"]').forEach(radio => {
            radio.addEventListener('change', (e) => this.selectDeliveryMethod(e.target.value));
        });
        
        // Edit buttons in review step
        document.querySelectorAll('[data-edit-step]').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const step = parseInt(e.target.dataset.editStep);
                this.goToStep(step);
            });
        });
    }

    /**
     * Initialize form validation
     */
    initFormValidation() {
        const form = document.getElementById('address-form');
        if (!form) return;

        // Add real-time validation
        const inputs = form.querySelectorAll('input, select');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => {
                if (input.classList.contains('is-invalid')) {
                    this.validateField(input);
                }
            });
        });
    }

    /**
     * Validate individual form field
     */
    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        // Remove existing validation classes
        field.classList.remove('is-valid', 'is-invalid');

        // Check if required field is empty
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required';
        }

        // Specific field validations
        switch (field.id) {
            case 'phoneNumber':
                if (value && !/^[\+]?[0-9\s\-\(\)]{8,}$/.test(value)) {
                    isValid = false;
                    errorMessage = 'Please enter a valid phone number';
                }
                break;
                
            case 'postalCode':
                if (value && !/^[0-9A-Za-z\s\-]{3,}$/.test(value)) {
                    isValid = false;
                    errorMessage = 'Please enter a valid postal code';
                }
                break;
        }

        // Update field appearance
        field.classList.add(isValid ? 'is-valid' : 'is-invalid');
        
        // Update error message
        const feedback = field.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = errorMessage;
        }

        return isValid;
    }

    /**
     * Validate entire address form
     */
    validateAddressForm() {
        const form = document.getElementById('address-form');
        if (!form) return false;

        const inputs = form.querySelectorAll('input[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        // Update continue button state
        const continueBtn = document.getElementById('continue-to-delivery');
        if (continueBtn) {
            continueBtn.disabled = !isValid;
        }

        return isValid;
    }

    /**
     * Show new address form (clear saved address selection)
     */
    showNewAddressForm() {
        // Clear saved address selection
        document.querySelectorAll('input[name="saved-address"]').forEach(radio => {
            radio.checked = false;
        });
        
        // Clear form
        document.getElementById('address-form').reset();
        
        // Update UI
        document.getElementById('address-form-title').textContent = 'New Shipping Address';
        document.getElementById('add-new-address-btn').style.display = 'none';
        
        // Clear checkout data
        this.checkoutData.address = null;
        
        this.validateAddressForm();
    }

    /**
     * Select delivery method
     */
    selectDeliveryMethod(method) {
        this.checkoutData.delivery = { method: method };
        
        // Update UI to show selected method
        document.querySelectorAll('.delivery-option').forEach(option => {
            option.classList.remove('active');
        });
        
        const selectedOption = document.querySelector(`[data-method="${method}"]`);
        if (selectedOption) {
            selectedOption.classList.add('active');
        }
        
        console.log('Selected delivery method:', method);
    }

    /**
     * Select payment method
     */
    selectPaymentMethod(method) {
        this.checkoutData.payment = { method: method };
        
        // Clear existing payment details
        const container = document.getElementById('payment-details-container');
        container.innerHTML = '';
        
        // Show payment-specific UI based on method
        if (method === 'stripe') {
            this.showCreditCardForm();
        } else if (method === 'paypal') {
            this.showPayPalInfo();
        }
        
        console.log('Selected payment method:', method);
    }

    /**
     * Show credit card form for Stripe
     */
    showCreditCardForm() {
        const container = document.getElementById('payment-details-container');
        container.innerHTML = `
            <div class="payment-form mb-4">
                <h6 class="mb-3">Credit Card Information</h6>
                <div class="p-3 rounded" style="background: var(--color-background); border: 1px solid var(--color-border);">
                    <div class="text-center text-muted">
                        <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor" class="mb-3">
                            <path d="M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z"/>
                        </svg>
                        <p class="mb-0">Secure credit card processing</p>
                        <small>Payment processing will be completed on the next page</small>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Show PayPal information
     */
    showPayPalInfo() {
        const container = document.getElementById('payment-details-container');
        container.innerHTML = `
            <div class="payment-form mb-4">
                <h6 class="mb-3">PayPal Payment</h6>
                <div class="p-3 rounded" style="background: var(--color-background); border: 1px solid var(--color-border);">
                    <div class="text-center text-muted">
                        <svg width="48" height="24" viewBox="0 0 24 24" fill="#00457C" class="mb-3">
                            <path d="M7.076 21.337H2.47l.396-2.5h1.51l1.18-7.503H2.47L2.874 9.1h4.606c.838 0 1.542.29 2.114.864.572.575.857 1.295.857 2.16 0 .734-.164 1.41-.493 2.028-.329.618-.78 1.122-1.354 1.513-.574.39-1.234.586-1.982.586H5.69l-.614 4.086zm2.638-7.59c0-.396-.128-.711-.385-.944-.256-.233-.6-.35-1.031-.35H7.52l-.55 3.47h.773c.432 0 .82-.134 1.166-.401.346-.268.52-.63.52-1.086v-.689z"/>
                        </svg>
                        <p class="mb-0">You'll be redirected to PayPal</p>
                        <small>Complete your payment securely with PayPal</small>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Navigate to a specific step
     */
    goToStep(stepNumber) {
        if (stepNumber < 1 || stepNumber > this.maxStep) return;
        
        // Track step navigation
        this.analytics.trackEvent('step_viewed', {
            step: stepNumber,
            step_name: this.getStepName(stepNumber),
            previous_step: this.currentStep,
            time_on_previous_step: Date.now() - this.stepStartTime
        });
        
        this.currentStep = stepNumber;
        this.stepStartTime = Date.now();
        this.showStep(stepNumber);
        this.updateProgressIndicator();
        
        // Update review section if going to review step
        if (stepNumber === 4) {
            this.updateReviewSection();
        }
    }

    /**
     * Get step name for analytics
     */
    getStepName(stepNumber) {
        const stepNames = {
            1: 'shipping_address',
            2: 'delivery_options', 
            3: 'payment_method',
            4: 'order_review'
        };
        return stepNames[stepNumber] || 'unknown';
    }

    /**
     * Proceed to next step with validation
     */
    async proceedToStep(stepNumber) {
        // Validate current step before proceeding
        if (!await this.validateCurrentStep()) {
            return;
        }
        
        // Capture data from current step
        await this.captureStepData();
        
        this.goToStep(stepNumber);
    }

    /**
     * Validate current step
     */
    async validateCurrentStep() {
        switch (this.currentStep) {
            case 1: // Address validation
                if (!this.validateAddressForm()) {
                    this.showError('Please complete all required address fields');
                    return false;
                }
                
                // Validate stock before proceeding
                if (!await this.validateStock()) {
                    return false;
                }
                return true;
                
            case 2: // Delivery validation
                if (!this.checkoutData.delivery) {
                    this.showError('Please select a delivery method');
                    return false;
                }
                return true;
                
            case 3: // Payment validation
                if (!this.checkoutData.payment) {
                    this.showError('Please select a payment method');
                    return false;
                }
                return true;
                
            case 4: // Final validation
                const termsCheck = document.getElementById('terms-acceptance');
                if (!termsCheck.checked) {
                    termsCheck.classList.add('is-invalid');
                    this.showError('Please accept the terms and conditions');
                    return false;
                }
                return true;
                
            default:
                return true;
        }
    }

    /**
     * Capture data from current step
     */
    async captureStepData() {
        switch (this.currentStep) {
            case 1: // Capture address data
                const form = document.getElementById('address-form');
                const formData = new FormData(form);
                
                this.checkoutData.address = {
                    firstName: formData.get('firstName'),
                    lastName: formData.get('lastName'),
                    street: formData.get('street'),
                    city: formData.get('city'),
                    postalCode: formData.get('postalCode'),
                    country: formData.get('country'),
                    phoneNumber: formData.get('phoneNumber'),
                    saveAddress: formData.get('saveAddress') === 'on'
                };
                
                // Save address if requested and user is authenticated
                if (this.checkoutData.address.saveAddress && this.isAuthenticated) {
                    await this.saveUserAddress(this.checkoutData.address);
                }
                break;
        }
    }

    /**
     * Save user address via API
     */
    async saveUserAddress(addressData) {
        if (!this.isAuthenticated) return;
        
        try {
            const response = await fetch('/api/v1/addresses', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify(addressData)
            });
            
            if (response.ok) {
                console.log('Address saved successfully');
                // Reload addresses to update the list
                await this.loadUserAddresses();
            }
        } catch (error) {
            console.warn('Could not save address:', error);
        }
    }

    /**
     * Validate stock availability
     */
    async validateStock() {
        try {
            const stockRequests = this.cartItems.map(item => ({
                productId: item.productId,
                quantity: item.quantity
            }));

            const response = await fetch('/api/v1/stock/validate-batch', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(stockRequests)
            });

            if (!response.ok) {
                throw new Error('Stock validation failed');
            }

            const result = await response.json();
            const unavailableItems = result.data.filter(item => !item.isAvailable);

            if (unavailableItems.length > 0) {
                this.showStockError(unavailableItems);
                return false;
            }

            return true;
        } catch (error) {
            console.error('Stock validation error:', error);
            this.showError('Unable to verify product availability. Please try again.');
            return false;
        }
    }

    /**
     * Show stock error with specific unavailable items
     */
    showStockError(unavailableItems) {
        const itemNames = unavailableItems.map(item => item.productName).join(', ');
        this.showError(`Some items are no longer available: ${itemNames}. Please update your cart and try again.`);
    }

    /**
     * Start periodic stock validation
     */
    startStockValidation() {
        // Validate stock every 30 seconds during checkout
        setInterval(() => {
            this.validateStock().catch(error => {
                console.warn('Background stock validation failed:', error);
            });
        }, 30000);
    }

    /**
     * Show specific step
     */
    showStep(stepNumber) {
        // Hide all steps
        document.querySelectorAll('.checkout-step').forEach(step => {
            step.style.display = 'none';
        });
        
        // Show target step
        const targetStep = document.getElementById(`step-${stepNumber}`);
        if (targetStep) {
            targetStep.style.display = 'block';
        }
        
        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    /**
     * Update progress indicator
     */
    updateProgressIndicator() {
        document.querySelectorAll('.step').forEach((step, index) => {
            const stepNumber = index + 1;
            step.classList.remove('active', 'completed');
            
            if (stepNumber < this.currentStep) {
                step.classList.add('completed');
            } else if (stepNumber === this.currentStep) {
                step.classList.add('active');
            }
        });
    }

    /**
     * Update review section with all collected data
     */
    updateReviewSection() {
        // Shipping address review
        if (this.checkoutData.address) {
            const addr = this.checkoutData.address;
            document.getElementById('review-shipping-address').innerHTML = `
                ${addr.firstName} ${addr.lastName}<br>
                ${addr.street}<br>
                ${addr.city}, ${addr.postalCode}<br>
                ${addr.country}<br>
                ${addr.phoneNumber}
            `;
        }
        
        // Delivery method review
        if (this.checkoutData.delivery) {
            document.getElementById('review-delivery-method').textContent = 
                this.checkoutData.delivery.method === 'standard' ? 'Standard Delivery (3-5 business days) - Free' : '';
        }
        
        // Payment method review
        if (this.checkoutData.payment) {
            const paymentText = this.checkoutData.payment.method === 'stripe' ? 'Credit Card' : 
                               this.checkoutData.payment.method === 'paypal' ? 'PayPal' : '';
            document.getElementById('review-payment-method').textContent = paymentText;
        }
    }

    /**
     * Complete the order
     */
    async completeOrder() {
        const button = document.getElementById('complete-order');
        const buttonText = button.querySelector('.button-text');
        const buttonSpinner = button.querySelector('.button-spinner');
        
        // Track order completion attempt
        this.analytics.trackEvent('order_completion_attempted', {
            cart_value: this.totals.total,
            cart_items_count: this.cartItems.length,
            user_type: this.isAuthenticated ? 'authenticated' : 'guest',
            payment_method: this.checkoutData.payment?.method,
            delivery_method: this.checkoutData.delivery?.method,
            total_checkout_time: Date.now() - this.startTime
        });
        
        // Show loading state
        button.disabled = true;
        buttonText.textContent = 'Processing...';
        buttonSpinner.style.display = 'inline-block';
        
        try {
            // Final validation
            if (!await this.validateCurrentStep()) {
                this.analytics.trackEvent('order_completion_failed', {
                    reason: 'validation_failed',
                    step: 'final_validation'
                });
                return;
            }
            
            // Create checkout session
            const checkoutRequest = {
                shippingAddress: this.checkoutData.address,
                deliveryMethod: this.checkoutData.delivery.method,
                paymentMethod: this.checkoutData.payment.method
            };
            
            const response = await fetch('/api/v1/checkout/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify(checkoutRequest)
            });
            
            if (!response.ok) {
                throw new Error(`Checkout failed: ${response.status}`);
            }
            
            const result = await response.json();
            
            // Track successful order completion
            this.analytics.trackEvent('order_completed', {
                checkout_id: result.data.id,
                cart_value: this.totals.total,
                cart_items_count: this.cartItems.length,
                user_type: this.isAuthenticated ? 'authenticated' : 'guest',
                payment_method: this.checkoutData.payment?.method,
                delivery_method: this.checkoutData.delivery?.method,
                total_checkout_time: Date.now() - this.startTime,
                conversion: true
            });
            
            // Redirect to success page
            window.location.href = `/Checkout/Success/${result.data.id}`;
            
        } catch (error) {
            console.error('Checkout completion error:', error);
            
            // Track failed order completion
            this.analytics.trackEvent('order_completion_failed', {
                reason: 'api_error',
                error_message: error.message,
                cart_value: this.totals.total,
                total_checkout_time: Date.now() - this.startTime
            });
            
            this.showError('Unable to complete your order. Please try again.');
            
            // Reset button state
            button.disabled = false;
            buttonText.textContent = 'Complete Order';
            buttonSpinner.style.display = 'none';
        }
    }

    /**
     * Show login prompt
     */
    showLoginPrompt() {
        if (confirm('Sign in to save addresses and view order history. Continue to login page?')) {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }
    }

    /**
     * Logout user
     */
    async logout() {
        try {
            const response = await fetch('/api/v1/account/logout', {
                method: 'POST',
                credentials: 'include'
            });
            
            if (response.ok) {
                // Refresh page to update authentication state
                window.location.reload();
            }
        } catch (error) {
            console.error('Logout error:', error);
        }
    }

    /**
     * Show error message to user
     */
    showError(message) {
        // Track error for analytics
        this.analytics.trackError('user_error', message, {
            current_step: this.currentStep,
            step_name: this.getStepName(this.currentStep)
        });
        
        // Create or update error alert
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
                </div>
            `;
            
            // Insert at the top of the current step
            const currentStepElement = document.getElementById(`step-${this.currentStep}`);
            currentStepElement.insertBefore(errorAlert, currentStepElement.firstChild);
        }
        
        errorAlert.querySelector('.error-message').textContent = message;
        errorAlert.scrollIntoView({ behavior: 'smooth', block: 'center' });
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (errorAlert.parentNode) {
                errorAlert.remove();
            }
        }, 5000);
    }

    /**
     * Show success message to user
     */
    showSuccess(message) {
        const successAlert = document.createElement('div');
        successAlert.className = 'alert alert-success';
        successAlert.innerHTML = `
            <div class="d-flex align-items-center">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" class="me-2">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
                <span>${message}</span>
            </div>
        `;
        
        const currentStepElement = document.getElementById(`step-${this.currentStep}`);
        currentStepElement.insertBefore(successAlert, currentStepElement.firstChild);
        
        setTimeout(() => successAlert.remove(), 3000);
    }
}

/**
 * Checkout Analytics Tracking System
 * Tracks user behavior and conversion metrics during checkout
 */
class CheckoutAnalytics {
    constructor() {
        this.events = [];
        this.sessionId = this.generateSessionId();
        this.startTime = Date.now();
    }

    /**
     * Track an analytics event
     */
    trackEvent(eventName, eventData = {}) {
        const event = {
            event: eventName,
            timestamp: new Date().toISOString(),
            session_id: this.sessionId,
            page_url: window.location.href,
            user_agent: navigator.userAgent,
            screen_resolution: `${window.screen.width}x${window.screen.height}`,
            viewport_size: `${window.innerWidth}x${window.innerHeight}`,
            ...eventData
        };

        this.events.push(event);
        console.log('Analytics Event:', eventName, event);

        // Send to analytics service (placeholder for future implementation)
        this.sendToAnalytics(event);

        // Store locally for debugging
        this.storeLocally(event);
    }

    /**
     * Send event to analytics service
     */
    async sendToAnalytics(event) {
        // Placeholder for future analytics service integration
        // Could be Google Analytics, Mixpanel, custom analytics API, etc.
        
        try {
            // Example: Send to custom analytics endpoint
            // await fetch('/api/v1/analytics/track', {
            //     method: 'POST',
            //     headers: { 'Content-Type': 'application/json' },
            //     body: JSON.stringify(event),
            //     credentials: 'include'
            // });

            // Example: Google Analytics 4 integration
            if (typeof gtag !== 'undefined') {
                gtag('event', event.event, {
                    checkout_session_id: event.session_id,
                    custom_parameter_1: event.cart_value || 0,
                    custom_parameter_2: event.user_type || 'unknown'
                });
            }

            // Example: Facebook Pixel integration
            if (typeof fbq !== 'undefined' && event.event === 'order_completed') {
                fbq('track', 'Purchase', {
                    value: event.cart_value,
                    currency: 'MAD',
                    content_type: 'product'
                });
            }

        } catch (error) {
            console.warn('Analytics tracking failed:', error);
        }
    }

    /**
     * Store event locally for debugging and offline analysis
     */
    storeLocally(event) {
        try {
            const stored = localStorage.getItem('aloud_checkout_analytics') || '[]';
            const events = JSON.parse(stored);
            events.push(event);
            
            // Keep only last 100 events to avoid storage issues
            if (events.length > 100) {
                events.splice(0, events.length - 100);
            }
            
            localStorage.setItem('aloud_checkout_analytics', JSON.stringify(events));
        } catch (error) {
            console.warn('Local analytics storage failed:', error);
        }
    }

    /**
     * Generate unique session ID
     */
    generateSessionId() {
        return 'checkout_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    /**
     * Track funnel conversion metrics
     */
    trackFunnelStep(stepName, stepData = {}) {
        this.trackEvent('funnel_step', {
            funnel_step: stepName,
            ...stepData
        });
    }

    /**
     * Track form interactions
     */
    trackFormInteraction(formName, fieldName, action, value = null) {
        this.trackEvent('form_interaction', {
            form_name: formName,
            field_name: fieldName,
            action: action, // 'focus', 'blur', 'change', 'submit'
            field_value: value ? value.length : null // Don't store actual values for privacy
        });
    }

    /**
     * Track errors and issues
     */
    trackError(errorType, errorMessage, context = {}) {
        this.trackEvent('checkout_error', {
            error_type: errorType,
            error_message: errorMessage,
            ...context
        });
    }

    /**
     * Get analytics summary for current session
     */
    getSessionSummary() {
        const sessionEvents = this.events.filter(e => e.session_id === this.sessionId);
        
        return {
            session_id: this.sessionId,
            session_duration: Date.now() - this.startTime,
            total_events: sessionEvents.length,
            events: sessionEvents,
            funnel_completion: this.calculateFunnelCompletion(sessionEvents),
            conversion_metrics: this.calculateConversionMetrics(sessionEvents)
        };
    }

    /**
     * Calculate funnel completion rates
     */
    calculateFunnelCompletion(events) {
        const funnelSteps = ['checkout_started', 'step_viewed', 'order_completion_attempted', 'order_completed'];
        const completion = {};

        funnelSteps.forEach(step => {
            completion[step] = events.some(e => e.event === step);
        });

        return completion;
    }

    /**
     * Calculate conversion metrics
     */
    calculateConversionMetrics(events) {
        const startedCheckout = events.some(e => e.event === 'checkout_started');
        const completedOrder = events.some(e => e.event === 'order_completed');
        const attemptedCompletion = events.some(e => e.event === 'order_completion_attempted');

        return {
            started_checkout: startedCheckout,
            attempted_completion: attemptedCompletion,
            completed_order: completedOrder,
            conversion_rate: startedCheckout && completedOrder ? 1 : 0,
            abandonment_point: this.identifyAbandonmentPoint(events)
        };
    }

    /**
     * Identify where users abandon the checkout process
     */
    identifyAbandonmentPoint(events) {
        const lastEvent = events[events.length - 1];
        if (!lastEvent) return 'initial';

        if (lastEvent.event === 'order_completed') return 'completed';
        if (lastEvent.event === 'order_completion_attempted') return 'payment_processing';
        
        const stepEvents = events.filter(e => e.event === 'step_viewed');
        const lastStep = stepEvents[stepEvents.length - 1];
        
        return lastStep ? lastStep.step_name : 'unknown';
    }

    /**
     * Export analytics data for analysis
     */
    exportAnalyticsData() {
        const data = {
            session_summary: this.getSessionSummary(),
            all_events: this.events,
            export_timestamp: new Date().toISOString()
        };

        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = `aloud-checkout-analytics-${this.sessionId}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
}

// Initialize checkout system when DOM is loaded
let checkoutManager;
document.addEventListener('DOMContentLoaded', function() {
    checkoutManager = new CheckoutManager();
});

// Export for global access
window.checkoutManager = checkoutManager;