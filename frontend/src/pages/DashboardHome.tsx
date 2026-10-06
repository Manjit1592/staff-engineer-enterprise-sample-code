import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ResponsiveContainer
} from "recharts";

import {
  Card,
  CardContent,
  Grid,
  Typography,
  Paper,
  Snackbar,
  Alert
} from "@mui/material";

import { useEffect, useState } from "react";
import connection from "../services/signalRService";

interface Props {
  productCount: number;
  orderCount: number;
  totalRevenue: number;
  lowStockCount: number;
  lowStockProducts: any[];
  recentOrders: any[];
  user: any;
}

const DashboardHome = ({
  productCount,
  orderCount,
  totalRevenue,
  lowStockCount,
  lowStockProducts,
  recentOrders,
  user
}: Props) => {
  const [openToast, setOpenToast] = useState(false);
  const [toastMessage, setToastMessage] = useState("");
  useEffect(() => {
    connection.off("OrderPlaced");

    connection.on("OrderPlaced", () => {
      setToastMessage("New order received");
      setOpenToast(true);
    });

    return () => {
      connection.off("OrderPlaced");
    };
  }, []);
  const chartData = [...recentOrders].reverse().map((x) => ({
    name: x.orderNumber,
    revenue: x.totalAmount
  }));

  return (
    <>
      <Typography variant="h4" gutterBottom fontWeight={700}>
        {user?.role === "Admin" ? "Enterprise Operations Dashboard" : "User Product Portal"}
      </Typography>

      <Grid container spacing={3} sx={{ mb: 4, mt: 2 }}>
        <Grid item xs={3}>
          <Card elevation={6} sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="h6">Total Products</Typography>
              <Typography variant="h3">{productCount}</Typography>
            </CardContent>
          </Card>
        </Grid>

        {user?.role === "Admin" && (
          <>
            <Grid item xs={3}>
              <Card elevation={6} sx={{ borderRadius: 3 }}>
                <CardContent>
                  <Typography variant="h6">Total Orders</Typography>
                  <Typography variant="h3">{orderCount}</Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={3}>
              <Card elevation={6} sx={{ borderRadius: 3 }}>
                <CardContent>
                  <Typography variant="h6">Total Revenue</Typography>
                  <Typography variant="h4">₹ {totalRevenue.toFixed(2)}</Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={3}>
              <Card elevation={6} sx={{ borderRadius: 3 }}>
                <CardContent>
                  <Typography variant="h6">Low Stock Alerts</Typography>
                  <Typography variant="h3">{lowStockCount}</Typography>
                </CardContent>
              </Card>
            </Grid>
          </>
        )}
      </Grid>

      {user?.role === "Admin" && (
        <>
          <Typography variant="h5" gutterBottom fontWeight={600}>
            Revenue Trend (Recent Orders)
          </Typography>

          <Paper sx={{ p: 3, mb: 4 }}>
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="revenue" strokeWidth={3} />
              </LineChart>
            </ResponsiveContainer>
          </Paper>

          <Grid container spacing={3}>
            <Grid item xs={6}>
              <Typography variant="h5" gutterBottom fontWeight={600}>
                Recent Orders
              </Typography>

              <Paper sx={{ p: 3 }}>
                {recentOrders.map((order) => (
                  <Paper
                    key={order.orderNumber}
                    sx={{
                      p: 2,
                      mb: 2,
                      borderRadius: 2,
                      backgroundColor: "#f8fafc"
                    }}
                  >
                    <Typography><b>Order Ref:</b> {order.orderNumber}</Typography>
                    <Typography><b>Customer:</b> {order.customerName}</Typography>
                    <Typography><b>Email:</b> {order.customerEmail}</Typography>
                    <Typography><b>Amount:</b> ₹ {order.totalAmount}</Typography>
                    <Typography><b>Status:</b> {order.status}</Typography>
                  </Paper>
                ))}
              </Paper>
            </Grid>

            <Grid item xs={6}>
              <Typography variant="h5" gutterBottom fontWeight={600}>
                Low Stock Product Alerts
              </Typography>

              <Paper sx={{ p: 3 }}>
                {lowStockProducts.length === 0 && (
                  <Typography>No low stock products.</Typography>
                )}

                {lowStockProducts.map((product) => (
                  <Paper
                    key={product.id}
                    sx={{
                      p: 2,
                      mb: 2,
                      borderRadius: 2,
                      backgroundColor: "#fff7ed"
                    }}
                  >
                    <Typography><b>Product:</b> {product.name}</Typography>
                    <Typography><b>SKU:</b> {product.sku}</Typography>
                    <Typography><b>Remaining Qty:</b> {product.stockQuantity}</Typography>
                  </Paper>
                ))}
              </Paper>
            </Grid>
          </Grid>
        </>
      )}

      {user?.role !== "Admin" && (
        <Typography variant="body1">
          Browse products and place orders from the Products section.
        </Typography>
      )}
          <Snackbar
        open={openToast}
        autoHideDuration={4000}
        onClose={() => setOpenToast(false)}
        anchorOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <Alert
          severity="success"
          variant="filled"
          onClose={() => setOpenToast(false)}
        >
          {toastMessage}
        </Alert>
      </Snackbar>
    </>
  );
};

export default DashboardHome;