// lắng nghe sự kiện -> khi các element trong body thay đổi.
new MutationObserver((mutations, observer) => {
    // gọi hàm khi UI chuyển sang trạng thai -> reconnect-failed
    //reconnect-rejected
    if (
        document.querySelector(
            "#components-reconnect-modal.components-reconnect-failed .m1-custom-failed h1"
        ) ||
        document.querySelector(
            "#components-reconnect-modal.components-reconnect-rejected .m1-custom-failed h1"
        )
    ) {
        const Reconnection = async () => {
            await fetch("");
            location.reload(); // bắt reload lại
        };
        observer.disconnect();
        Reconnection();
        setInterval(Reconnection, 3500);
    }
}).observe(document.getElementById("components-reconnect-modal"), {
    attributes: true,
    characterData: true,
    childList: true,
    subtree: true,
    attributeOldValue: true,
    characterDataOldValue: true,
});

//print
function printHtml(html) {
    var printWindow = window.open("", "_blank");
    if (printWindow) {
        printWindow.document.open();
        printWindow.document.write(html);
        printWindow.document.close();
        // Chờ cho nội dung HTML được tải xong trước khi thực hiện in
        printWindow.onload = function () {
            printWindow.print();
            printWindow.close();
        };
    }
    else {
        // Xử lý trường hợp cửa sổ in không mở được
        alert("Không thể mở cửa sổ in. Vui lòng kiểm tra cài đặt trình duyệt của bạn.");
    }
}