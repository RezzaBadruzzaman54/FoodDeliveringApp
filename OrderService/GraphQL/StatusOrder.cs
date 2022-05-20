namespace OrderService.GraphQL
{
    public class StatusOrder
    {
        public static readonly string Waiting = "Menunggu Konfirmasi";
        public static readonly string OnProses = "Pesanan Sedang Diproses";
        public static readonly string OnDelivery = "Pesanan Sedang Diantar";
        public static readonly string Completed = "Pesanan Selesai";
    }
}
