using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class PaymentInEntity : OperationEntity, ISupportPartialPriceUpdate, IDataEntity<PaymentInEntity, PaymentIn>
    {
        [StringLength(64)]
        public string OrganizationId { get; set; }
        [StringLength(255)]
        public string OrganizationName { get; set; }

        [Required]
        [StringLength(64)]
        public string CustomerId { get; set; }
        [StringLength(255)]
        public string CustomerName { get; set; }

        public DateTime? IncomingDate { get; set; }
        [StringLength(1024)]
        public string Purpose { get; set; }
        [StringLength(64)]
        public string GatewayCode { get; set; }

        public DateTime? AuthorizedDate { get; set; }
        public DateTime? CapturedDate { get; set; }
        public DateTime? VoidedDate { get; set; }

        [StringLength(64)]
        public string TaxType { get; set; }

        [Column(TypeName = "Money")]
        public decimal Price { get; set; }
        [Column(TypeName = "Money")]
        public decimal PriceWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountAmountWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal Total { get; set; }
        [Column(TypeName = "Money")]
        public decimal TotalWithTax { get; set; }

        [Column(TypeName = "Money")]
        public decimal TaxTotal { get; set; }
        public decimal TaxPercentRate { get; set; }

        [StringLength(64)]
        public string VendorId { get; set; }

        #region Navigation Properties

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        public string ShipmentId { get; set; }
        public virtual ShipmentEntity Shipment { get; set; }

        public virtual ObservableCollection<AddressEntity> Addresses { get; set; } = new NullCollection<AddressEntity>();

        public virtual ObservableCollection<PaymentGatewayTransactionEntity> Transactions { get; set; }
            = new NullCollection<PaymentGatewayTransactionEntity>();

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; } = new NullCollection<DiscountEntity>();

        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; } = new NullCollection<TaxDetailEntity>();

        public virtual ObservableCollection<FeeDetailEntity> FeeDetails { get; set; } = new NullCollection<FeeDetailEntity>();

        public virtual ObservableCollection<RefundEntity> Refunds { get; set; } = new NullCollection<RefundEntity>();

        public virtual ObservableCollection<CaptureEntity> Captures { get; set; } = new NullCollection<CaptureEntity>();

        public virtual ObservableCollection<OrderDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<OrderDynamicPropertyObjectValueEntity>();

        #endregion

        public virtual PaymentIn ToModel(PaymentIn payment)
        {
            return (PaymentIn)ToModel((OrderOperation)payment);
        }

        public override OrderOperation ToModel(OrderOperation operation)
        {
            var payment = operation as PaymentIn;
            if (payment == null)
            {
                throw new ArgumentException(@"operation argument must be of type PaymentIn", nameof(operation));
            }

            if (!Addresses.IsNullOrEmpty())
            {
                payment.BillingAddress = Addresses.First().ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
            }

            payment.OrderId = CustomerOrderId;

            payment.Price = Price;
            payment.PriceWithTax = PriceWithTax;
            payment.DiscountAmount = DiscountAmount;
            payment.DiscountAmountWithTax = DiscountAmountWithTax;
            payment.TaxType = TaxType;
            payment.TaxPercentRate = TaxPercentRate;
            payment.TaxTotal = TaxTotal;
            payment.Total = Total;
            payment.TotalWithTax = TotalWithTax;

            payment.CustomerId = CustomerId;
            payment.CustomerName = CustomerName;
            payment.OrganizationId = OrganizationId;
            payment.OrganizationName = OrganizationName;
            payment.GatewayCode = GatewayCode;
            payment.Purpose = Purpose;
            payment.OuterId = OuterId;
            payment.Status = Status;
            payment.AuthorizedDate = AuthorizedDate;
            payment.CapturedDate = CapturedDate;
            payment.VoidedDate = VoidedDate;
            payment.IsCancelled = IsCancelled;
            payment.CancelledDate = CancelledDate;
            payment.CancelReason = CancelReason;
            payment.Sum = Sum;
            payment.VendorId = VendorId;

            payment.Transactions = Transactions.Select(x => x.ToModel(AbstractTypeFactory<PaymentGatewayTransaction>.TryCreateInstance())).ToList();
            payment.TaxDetails = TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            payment.FeeDetails = FeeDetails.Select(x => x.ToModel(AbstractTypeFactory<FeeDetail>.TryCreateInstance())).ToList();
            payment.Discounts = Discounts.Select(x => x.ToModel(AbstractTypeFactory<Discount>.TryCreateInstance())).ToList();

            payment.Refunds = Refunds.Select(x => x.ToModel(AbstractTypeFactory<Refund>.TryCreateInstance())).ToList();
            payment.Captures = Captures.Select(x => x.ToModel(AbstractTypeFactory<Capture>.TryCreateInstance())).ToList();

            payment.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            base.ToModel(payment);

            payment.PaymentStatus = EnumUtility.SafeParse(Status, PaymentStatus.Custom);

            return payment;
        }

        public virtual PaymentInEntity FromModel(PaymentIn payment, PrimaryKeyResolvingMap pkMap)
        {
            return (PaymentInEntity)FromModel((OrderOperation)payment, pkMap);
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            var payment = operation as PaymentIn;
            if (payment == null)
            {
                throw new ArgumentException(@"operation argument must be of type PaymentIn", nameof(operation));
            }

            base.FromModel(payment, pkMap);

            Price = payment.Price;
            PriceWithTax = payment.PriceWithTax;
            DiscountAmount = payment.DiscountAmount;
            DiscountAmountWithTax = payment.DiscountAmountWithTax;
            TaxType = payment.TaxType;
            TaxPercentRate = payment.TaxPercentRate;
            TaxTotal = payment.TaxTotal;
            Total = payment.Total;
            TotalWithTax = payment.TotalWithTax;

            CustomerId = payment.CustomerId;
            CustomerName = payment.CustomerName;
            OrganizationId = payment.OrganizationId;
            OrganizationName = payment.OrganizationName;
            GatewayCode = payment.GatewayCode;
            Purpose = payment.Purpose;
            OuterId = payment.OuterId;
            Status = payment.Status;
            AuthorizedDate = payment.AuthorizedDate;
            CapturedDate = payment.CapturedDate;
            VoidedDate = payment.VoidedDate;
            IsCancelled = payment.IsCancelled;
            CancelledDate = payment.CancelledDate;
            CancelReason = payment.CancelReason;
            Sum = payment.Sum;
            VendorId = payment.VendorId;

            if (payment.PaymentMethod != null)
            {
                GatewayCode = payment.PaymentMethod != null ? payment.PaymentMethod.Code : payment.GatewayCode;
            }

            Addresses = payment.BillingAddress != null
                ? new ObservableCollection<AddressEntity>(new[] { AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(payment.BillingAddress) })
                : new ObservableCollection<AddressEntity>();

            if (payment.TaxDetails != null)
            {
                TaxDetails = new ObservableCollection<TaxDetailEntity>(payment.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (payment.FeeDetails != null)
            {
                FeeDetails = new ObservableCollection<FeeDetailEntity>(payment.FeeDetails.Select(x => AbstractTypeFactory<FeeDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (payment.Discounts != null)
            {
                Discounts = new ObservableCollection<DiscountEntity>(payment.Discounts.Select(x => AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(x)));
            }

            if (payment.Transactions != null)
            {
                Transactions = new ObservableCollection<PaymentGatewayTransactionEntity>(payment.Transactions.Select(x => AbstractTypeFactory<PaymentGatewayTransactionEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (payment.Refunds != null)
            {
                Refunds = new ObservableCollection<RefundEntity>(payment.Refunds.Select(x => AbstractTypeFactory<RefundEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (payment.Captures != null)
            {
                Captures = new ObservableCollection<CaptureEntity>(payment.Captures.Select(x => AbstractTypeFactory<CaptureEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (payment.Status.IsNullOrEmpty())
            {
                Status = payment.PaymentStatus.ToString();
            }

            if (payment.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<OrderDynamicPropertyObjectValueEntity>(payment.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<OrderDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, payment, p))).OfType<OrderDynamicPropertyObjectValueEntity>());
            }

            return this;
        }

        public virtual void Patch(PaymentInEntity target)
        {
            Patch((OperationEntity)target);
        }

        public override void Patch(OperationEntity target)
        {
            var payment = target as PaymentInEntity;
            if (payment == null)
                throw new ArgumentException(@"target argument must be of type PaymentInEntity", nameof(target));

            // Patch prices if there are non 0 prices in the patching entity, or all patched entity prices are 0
            var isNeedPatch = GetNonCalculatablePrices().Any(x => x != 0m) || payment.GetNonCalculatablePrices().All(x => x == 0m);

            NeedPatchSum = isNeedPatch;
            base.Patch(payment);

            payment.TaxType = TaxType;
            payment.CustomerId = CustomerId;
            payment.CustomerName = CustomerName;
            payment.OrganizationId = OrganizationId;
            payment.OrganizationName = OrganizationName;
            payment.GatewayCode = GatewayCode;
            payment.Purpose = Purpose;
            payment.OuterId = OuterId;
            payment.Status = Status;
            payment.AuthorizedDate = AuthorizedDate;
            payment.CapturedDate = CapturedDate;
            payment.VoidedDate = VoidedDate;
            payment.IsCancelled = IsCancelled;
            payment.CancelledDate = CancelledDate;
            payment.CancelReason = CancelReason;
            payment.VendorId = VendorId;

            if (isNeedPatch)
            {
                payment.Price = Price;
                payment.PriceWithTax = PriceWithTax;
                payment.DiscountAmount = DiscountAmount;
                payment.DiscountAmountWithTax = DiscountAmountWithTax;
                payment.TaxPercentRate = TaxPercentRate;
                payment.TaxTotal = TaxTotal;
                payment.Total = Total;
                payment.TotalWithTax = TotalWithTax;
                payment.Sum = Sum;
            }


            if (!Addresses.IsNullCollection())
            {
                Addresses.Patch(payment.Addresses, (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }

            if (!TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                TaxDetails.Patch(payment.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }

            if (!FeeDetails.IsNullCollection())
            {
                var feeDetailComparer = AnonymousComparer.Create((FeeDetailEntity x) => x.FeeId);
                FeeDetails.Patch(payment.FeeDetails, feeDetailComparer, (sourceFeeDetail, targetFeeDetail) => sourceFeeDetail.Patch(targetFeeDetail));
            }

            if (!Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                Discounts.Patch(payment.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }

            if (!Transactions.IsNullCollection())
            {
                Transactions.Patch(payment.Transactions, (sourceTran, targetTran) => sourceTran.Patch(targetTran));
            }

            if (!Refunds.IsNullCollection())
            {
                Refunds.Patch(payment.Refunds, (sourceRefund, targetRefund) => sourceRefund.Patch(targetRefund));
            }

            if (!Captures.IsNullCollection())
            {
                Captures.Patch(payment.Captures, (sourceCapture, targetCapture) => sourceCapture.Patch(targetCapture));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(payment.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }

        public virtual void ResetPrices()
        {
            Price = 0m;
            PriceWithTax = 0m;
            DiscountAmount = 0m;
            DiscountAmountWithTax = 0m;
            Total = 0m;
            TotalWithTax = 0m;
            TaxTotal = 0m;
            TaxPercentRate = 0m;
            Sum = 0m;
        }

        public virtual IEnumerable<decimal> GetNonCalculatablePrices()
        {
            yield return TaxPercentRate;
            yield return Price;
            yield return DiscountAmount;
            yield return Sum;
        }
    }
}
