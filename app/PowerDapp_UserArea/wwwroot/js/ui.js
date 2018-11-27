var p = $.proxy;

$(function () {

    $(document)
        .on('change', '.masked-select select', function () {
            $(this).parent().find('input[type=text]').val($(this).find(':selected').text());
        });

    BuyTokens.Init();
});

var BuyTokens = {

    _currencySelect: null,
    _amountInput: null,
    _paymentInput: null,
    _form: null,
    _paymentInfo: null,
    _buyButton: null,

    csi: null,
    orderId: null,

    Init: function () {
        if (!$('form.buy-form').length) {
            return;
        }

        this._currencySelect = $('#CurrencyCode');
        this._amountInput = $('#TokensAmount');
        this._paymentInput = $('#PaymentAmount');
        this._form = $('form.buy-form');
        this._paymentInfo = $('#PaymentInfo');
        this._buyButton = $('#btnBuy');

        $(document)
            .on('change', '#CurrencyCode', p(this.Recalculate, this))
            .on('input', '#TokensAmount', p(this.Recalculate, this))
            .on('submit', '.buy-form', p(this.Buy, this));

        $('#PaymentModal').modal({
            backdrop: 'static',
            show: false
        });
        $('#PaymentModal').on('hide.bs.modal', p(this.onBuyComplete, this));

        $('#PaymentReceivedModal').modal({
            backdrop: 'static',
            show: false
        });
    },

    Recalculate: function () {
        var rate = parseFloat(this._currencySelect.find(':selected').data('rate'));
        this._paymentInput.val((this._amountInput.val() * rate).toFixed(6).toString());
    },

    Buy: function () {

        this._buyButton.html('<span class="fa fa-spinner fa-spin"></span>');
        
        $.ajax({
            url: this._form.attr('action'),
            type: 'post',
            data: {
                TokensAmount: this._amountInput.val() * 1,
                CurrencyCode: this._currencySelect.val(),
                __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
            },
            success: p(this.onBuySuccess, this),
            complete: p(this.onBuyComplete, this)
        });

        return false;
    },

    CheckStatus: function () {

        var icon = $('<span class="fa fa-refresh fa-spin"></span>');
        icon.css({
            position: 'absolute',
            right: '0.5rem',
            top: '0.5rem'
        });

        this._paymentInfo.append(icon);

        $.ajax({
            url: '/Service/CheckStatus/' + this.orderId,
            success: p(this.onCheckStatusSuccess, this),
            complete: p(this.onCheckStatusComplete, this)
        });
    },

    onCheckStatusSuccess: function (data) {

        if (data.isSuccess && data.isPaymentReceived) {
            clearInterval(this.csi);

            this._paymentInfo.removeClass('alert-primary bg-primary').addClass('alert-success bg-success');
            this._paymentInfo.html('Your payment was successfully received');

            this._buyButton.attr('disabled', false);
            this._amountInput.attr('disabled', false);

            $('#PaymentReceivedModal')
                .find('#ReceivedText').html(data.price).end()
                .find('#ReceivedCurrencyText').html(data.currencyCode).end()
                .find('#ReceivedTokensText').html(data.tokensAmount).end()
                .modal('show');
        }
    },

    onCheckStatusComplete: function () {
        this._paymentInfo.find('.fa').remove();
    },

    onBuySuccess: function (data) {
        if (data.isSuccess) {
            $('#qrcode').html('');
            $('#qrcode').qrcode({
                width: 130,
                height: 130,
                background: '#98ca42',
                text: data.targetWallet
            });

            $('#PaymentModal')
                .find('#CurrencyText').html(data.currencyCode).end()
                .find('#PriceText').html(data.price).end()
                .find('#TokensText').html(data.tokensAmount).end()
                .find('#AddressText').html(data.targetWallet);

            $('#GasLimitText').toggle(data.currencyCode === 'ETH');

            $('#PaymentModal').modal('show');
            this._buyButton.attr('disabled', true);
            this._amountInput.attr('disabled', true);

            this.orderId = data.id;
            this.csi = setInterval(p(this.CheckStatus, this), 30000);
        }
    },

    onBuyComplete: function () {
        this._buyButton.html("Buy");
    }
};