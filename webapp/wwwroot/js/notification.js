toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveOrderNotification", (notification) => {
    showNotification(notification);
});

connection.start().catch(err => console.error("Error connecting to notification hub:", err));

function showNotification(notification) {
    if ("Notification" in window) {
        if (Notification.permission === "granted") {
            createBrowserNotification(notification);
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(permission => {
                if (permission === "granted") {
                    createBrowserNotification(notification);
                }
            });
        }
    }

    showToastrNotification(notification);
}

function getOrderDetailUrl(status, orderId) {
    if (status === "NewOrder") {
        return `/Courier/OrderDetail/${orderId}`;
    }
    return `/Customer/OrderDetail/${orderId}`;
}

function createBrowserNotification(notification) {
    const title = notification.status === "NewOrder" ? "New Order Available" : "Order Update";
    const options = {
        body: notification.message,
        icon: "/favicon.ico",
        badge: "/favicon.ico",
        tag: `order-${notification.orderId}`,
        requireInteraction: notification.status === "NewOrder",
        data: {
            orderId: notification.orderId,
            url: getOrderDetailUrl(notification.status, notification.orderId)
        }
    };

    const browserNotification = new Notification(title, options);

    browserNotification.onclick = function(event) {
        event.preventDefault();
        window.open(event.target.data.url, '_blank');
        browserNotification.close();
    };
}

function showToastrNotification(notification) {
    const orderLink = `<br><a href="${getOrderDetailUrl(notification.status, notification.orderId)}" style="color: #fff; text-decoration: underline;">View Order</a>`;
    
    switch(notification.status) {
        case "NewOrder":
            toastr.warning(notification.message + orderLink, "New Order Available", {
                timeOut: 10000,
                extendedTimeOut: 5000
            });
            break;
        case "Accepted":
            toastr.success(notification.message + orderLink, "Order Accepted");
            break;
        case "PickedUp":
            toastr.info(notification.message + orderLink, "Order Picked Up");
            break;
        case "Delivered":
            toastr.success(notification.message + orderLink, "Order Delivered");
            break;
        default:
            toastr.info(notification.message + orderLink, "Order Update");
    }
}

if ("Notification" in window && Notification.permission === "default") {
    Notification.requestPermission();
}