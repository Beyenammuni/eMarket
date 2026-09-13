using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Sales.Payments.Commands.CreatePayment
{
    public sealed record CreatePaymentRequest(
      string ReturnUrl);
}
