import { useEffect, useState } from "react";
import {
  Paper,
  Typography,
  Chip,
  Select,
  MenuItem
} from "@mui/material";

import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";

import apiClient from "../api/apiClient";

import {
  startSignalRConnection,
  subscribeToOrderUpdates
} from "../services/signalRService";

const OrdersPage = () => {
  const [orders, setOrders] = useState<any[]>([]);

  const loadOrders = () => {
    apiClient
  .get("/orders/all", {
    headers: {
      "Cache-Control": "no-cache"
    }
  })
  .then((res) => {
    setOrders([...res.data.data]);
  });
  };

  useEffect(() => {
  loadOrders();

  startSignalRConnection();

  subscribeToOrderUpdates(() => {
    console.log(
      "Refreshing orders from realtime event"
    );

    loadOrders();
  });
}, []);

  const updateOrderStatus = async (
    orderNumber: string,
    status: string
  ) => {
    const order = orders.find(
      (x) => x.orderNumber === orderNumber
    );

    if (!order) {
      return;
    }

    try {
      await apiClient.patch(
        `/orders/status/${order.id}`,
        {
          status
        }
      );

      loadOrders();
    } catch (err) {
      console.error(err);
    }
  };

  const rows = orders.map((o) => ({
    id: o.id,
    orderNumber: o.orderNumber,
    customerName: o.customerName,
    customerEmail: o.customerEmail,
    totalAmount: o.totalAmount,
    status: o.status,
    createdOnUtc: new Date(o.createdOnUtc).toLocaleString()
  }));

  const getStatusColor = (status: string) => {
    switch (status) {
      case "PLACED":
        return "primary";

      case "PROCESSING":
        return "warning";

      case "SHIPPED":
        return "info";

      case "DELIVERED":
        return "success";

      case "CANCELLED":
        return "error";

      default:
        return "default";
    }
  };

  const columns: GridColDef[] = [
    {
      field: "orderNumber",
      headerName: "Order Ref",
      flex: 1
    },
    {
      field: "customerName",
      headerName: "Customer Name",
      flex: 1.2
    },
    {
      field: "customerEmail",
      headerName: "Customer Email",
      flex: 1.5
    },
    {
      field: "totalAmount",
      headerName: "Amount",
      flex: 0.8
    },
    {
      field: "status",
      headerName: "Status",
      flex: 1.2,
      renderCell: (params) => (
        <Select
  size="small"
  onClick={(e) => e.stopPropagation()}
          value={params.row.status}
          onChange={(e) =>
            updateOrderStatus(
              params.row.orderNumber,
              e.target.value
            )
          }
        >
          <MenuItem value="PLACED">PLACED</MenuItem>
          <MenuItem value="PROCESSING">
            PROCESSING
          </MenuItem>
          <MenuItem value="SHIPPED">
            SHIPPED
          </MenuItem>
          <MenuItem value="DELIVERED">
            DELIVERED
          </MenuItem>
          <MenuItem value="CANCELLED">
            CANCELLED
          </MenuItem>
        </Select>
      )
    },
    {
      field: "statusChip",
      headerName: "Lifecycle",
      flex: 1,
      renderCell: (params) => (
        <Chip
          label={params.row.status}
          color={getStatusColor(params.row.status)}
        />
      )
    },
    {
      field: "createdOnUtc",
      headerName: "Created On",
      flex: 1.4
    }
  ];

  return (
    <>
      <Typography
        variant="h4"
        gutterBottom
        fontWeight={600}
      >
        Orders Monitoring
      </Typography>

      <Paper sx={{ height: 550, p: 2 }}>
        <DataGrid
          rows={rows}
          columns={columns}
        />
      </Paper>
    </>
  );
};

export default OrdersPage;