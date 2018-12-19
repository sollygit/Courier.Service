using Courier.Service.Interfaces;
using Courier.Service.Models;
using Courier.Service.Models.Notification;
using Courier.Service.Models.ParcelLabel;
using Courier.Service.Models.ParcelPickup;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Courier.Service.Services
{
    public class RequestHandlerService : IHostedService, IObserver<CourierRequest>
    {
        private IDisposable unsubscriber;
        private readonly ILogger<RequestHandlerService> logger;
        private readonly IEventBus<CourierRequest> bus;
        private readonly ICourierDetailsService courierDetailsService;
        private readonly IParcelPickupService parcelService;
        private readonly IParcelLabelService labelService;
        private readonly IACEService aceService;
        private readonly INotificationService notificationService;

        public RequestHandlerService(
            ILogger<RequestHandlerService> logger,
            IEventBus<CourierRequest> bus,
            ICourierDetailsService courierDetailsService,
            IParcelPickupService parcelService,
            IParcelLabelService labelService,
            IACEService aceService,
            INotificationService notificationService)
        {
            this.logger = logger;
            this.bus = bus;
            this.courierDetailsService = courierDetailsService;
            this.parcelService = parcelService;
            this.labelService = labelService;
            this.aceService = aceService;
            this.notificationService = notificationService;
        }

        public virtual void OnNext(CourierRequest request)
        {
            _ = Process(request);
        }

        public virtual void OnCompleted()
        {
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void Subscribe(IObservable<CourierRequest> provider)
        {
            unsubscriber = bus.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Subscribe(bus);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Unsubscribe();
            return Task.CompletedTask;
        }

        public async Task Process(CourierRequest request)
        {
            var consignment = new Consignment();
            var courierDetails = new CourierDetails();

            try
            {
                // Get Courier Details 
                courierDetails = await courierDetailsService.Get(request.BranchId, request.DeliveryType);

                // Parcel Pickup
                var parcelPickupRequest = new ParcelPickupRequest
                {
                    Carrier = request.Carrier,
                    Caller = request.Caller,
                    Service_Code = courierDetails.ServiceCode,
                    Pickup_Date_Time = request.Parcel_Pickup_Date_Time,
                    Parcel_Quantity = request.Parcel_Quantity,
                    Pickup_Address = new PickupAddress { Phone = request.Parcel_Pickup_Address.Phone, Site_Code = courierDetails.SiteCode },
                    Delivery_Address = request.Parcel_Delivery_Address
                };

                // Create a jobNumber
                var jobNumber = await parcelService.ParcelPickup(parcelPickupRequest, courierDetails);

                var parcelLabelRequest = new ParcelLabelRequest
                {
                    Carrier = request.Carrier,
                    Logo_Id = courierDetails.LogoId,
                    Job_Number = Convert.ToInt32(jobNumber),
                    Sender_Details = request.Label_Sender_Details,
                    Receiver_Details = request.Label_Receiver_Details,
                    Pickup_Address = new PickAddress { Site_Code = Convert.ToInt32(courierDetails.SiteCode) },
                    Delivery_Address = request.Label_Delivery_Address
                };

                // Create a label
                consignment = await labelService.Create(parcelLabelRequest, courierDetails);

                // Get the label ConsignmentURL
                consignment.ConsignmentURL = await labelService.GetStatus(consignment.ConsignmentId, courierDetails);

                // Update ACE with the ConsignmentUrl (NPLU)
                // await aceService.UpdateParcelLabel(request.BranchId.ToString(), request.FullOrderNumber, consignment, courierDetails.Username);

                // Setup Label is Ready notification
                var notificationRequest = new NotificationRequest
                {
                    TransactionId = DateTime.Now.Ticks.ToString(),
                    OrderNo = request.FullOrderNumber,
                    BranchId = request.BranchId,
                    BranchName = request.Caller,
                    CustomerId = request.Label_Receiver_Details.Name,
                    CustomerEmail = "solly.fathi@placemakers.co.nz"
                };

                // Send the notification
                await notificationService.Send(notificationRequest, courierDetails.Username, consignment.ConsignmentId);
            }

            catch (ServiceException ex)
            {
                // Update ACE of any consignment related errors
                consignment.Details = ex.Message;
                await aceService.UpdateParcelLabel(request.BranchId.ToString(), request.FullOrderNumber, consignment, courierDetails.Username);
            }

            catch (Exception ex)
            {
                // Update ACE of any other errors thrown by the service
                consignment.Details = ex.Message;
                await aceService.UpdateParcelLabel(request.BranchId.ToString(), request.FullOrderNumber, consignment, courierDetails.Username);
            }
        }
    }
}
