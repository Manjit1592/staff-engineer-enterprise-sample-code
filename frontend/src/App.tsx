import { useEffect, useState } from "react";
import LoginPage from "./pages/LoginPage";
import AdminLayout from "./layouts/AdminLayout";
import ProductsPage from "./pages/ProductsPage";
import OrdersPage from "./pages/OrdersPage";
import DashboardHome from "./pages/DashboardHome";
import apiClient from "./api/apiClient";
import { getUserFromToken } from "./utils/jwtHelper";
import type { UserClaims } from "./utils/jwtHelper";

import {
  startSignalRConnection,
  subscribeToOrderUpdates
} from "./services/signalRService";

function App() {
  const [loggedIn, setLoggedIn] = useState(!!localStorage.getItem("token"));
  const [page, setPage] = useState("dashboard");

  const [productCount, setProductCount] = useState(0);
  const [orderCount, setOrderCount] = useState(0);
  const [totalRevenue, setTotalRevenue] = useState(0);
  const [lowStockCount, setLowStockCount] = useState(0);
  const [lowStockProducts, setLowStockProducts] = useState<any[]>([]);
  const [recentOrders, setRecentOrders] = useState<any[]>([]);

  const [user, setUser] = useState<UserClaims | null>(getUserFromToken());

  const loadDashboardCounts = () => {
    const currentUser = getUserFromToken();
    setUser(currentUser);

    apiClient.get("/products/all").then((res) => {
      const products = res.data.data;
      const lowProducts = products.filter((x: any) => x.stockQuantity < 10);

      setProductCount(products.length);
      setLowStockCount(lowProducts.length);
      setLowStockProducts(lowProducts.slice(0, 5));
    });

    if (currentUser?.role === "Admin") {
      apiClient.get("/orders/all").then((res) => {
        const orders = res.data.data;

        setOrderCount(orders.length);

        const revenue = orders.reduce(
          (sum: number, x: any) => sum + x.totalAmount,
          0
        );

        setTotalRevenue(revenue);

        const sorted = [...orders]
          .sort(
            (a, b) =>
              new Date(b.createdOnUtc).getTime() -
              new Date(a.createdOnUtc).getTime()
          )
          .slice(0, 5);

        setRecentOrders(sorted);
      });
    }
  };

  useEffect(() => {
    if (loggedIn) {
      loadDashboardCounts();

      startSignalRConnection();

      subscribeToOrderUpdates(() => {
        console.log("Realtime order update received");
        loadDashboardCounts();
      });
    }
  }, [loggedIn]);

  if (!loggedIn) {
    return <LoginPage onLoginSuccess={() => setLoggedIn(true)} />;
  }

  return (
    <AdminLayout onNavigate={setPage} user={user}>
      {page === "dashboard" && (
        <DashboardHome
          productCount={productCount}
          orderCount={orderCount}
          totalRevenue={totalRevenue}
          lowStockCount={lowStockCount}
          lowStockProducts={lowStockProducts}
          recentOrders={recentOrders}
          user={user}
        />
      )}

      {page === "products" && <ProductsPage />}

      {page === "orders" && user?.role === "Admin" && <OrdersPage />}
    </AdminLayout>
  );
}

export default App;