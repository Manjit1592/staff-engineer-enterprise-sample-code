import { useEffect, useState } from "react";
import { Paper, Typography, Button, Box } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";
import apiClient from "../api/apiClient";
import ProductCreateDialog from "./ProductCreateDialog";
import ProductOrderDialog from "./ProductOrderDialog";
import { getUserFromToken } from "../utils/jwtHelper";

const ProductsPage = () => {
  const [products, setProducts] = useState<any[]>([]);
  const [openCreate, setOpenCreate] = useState(false);
  const [dialogKey, setDialogKey] = useState(0);
  const [gridKey, setGridKey] = useState(0);

  const [openOrder, setOpenOrder] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<any>(null);

  const user = getUserFromToken();

  const loadProducts = () => {
    apiClient.get("/products/all").then((res) => {
      setProducts(res.data.data);
      setGridKey((prev) => prev + 1);
    });
  };

  useEffect(() => {
    loadProducts();
  }, []);

  const handleCloseCreateDialog = () => {
    setOpenCreate(false);
    setDialogKey((prev) => prev + 1);

    setTimeout(() => {
      loadProducts();
    }, 700);
  };

  const handleCloseOrderDialog = () => {
    setOpenOrder(false);
    setSelectedProduct(null);

    setTimeout(() => {
      loadProducts();
    }, 2000);
  };

  const userColumns: GridColDef[] = [
    { field: "name", headerName: "Product Name", flex: 1.5 },
    { field: "sku", headerName: "SKU", flex: 1 },
    { field: "price", headerName: "Price", flex: 1 },
    { field: "stockQuantity", headerName: "Stock Qty", flex: 1 },
    {
      field: "action",
      headerName: "Action",
      flex: 1,
      renderCell: (params) => (
        <Button
          variant="contained"
          size="small"
          onClick={() => {
            setSelectedProduct(params.row);
            setOpenOrder(true);
          }}
        >
          Place Order
        </Button>
      )
    }
  ];

  const adminColumns: GridColDef[] = [
    { field: "name", headerName: "Product Name", flex: 1.5 },
    { field: "sku", headerName: "SKU", flex: 1 },
    { field: "price", headerName: "Price", flex: 1 },
    { field: "stockQuantity", headerName: "Stock Qty", flex: 1 },
    { field: "isActive", headerName: "Active", flex: 1 }
  ];
console.log(products);
  return (
    <>
      <Box sx={{ display: "flex", justifyContent: "space-between", mb: 2 }}>
        <Typography variant="h4" gutterBottom fontWeight={600}>
          Products Inventory
        </Typography>

        {user?.role === "Admin" && (
          <Button variant="contained" onClick={() => setOpenCreate(true)}>
            Add Product
          </Button>
        )}
      </Box>

      <Paper sx={{ height: 450, p: 2 }}>
        <DataGrid
  key={gridKey}
  rows={products}
  getRowId={(row) => row.id}
  columns={user?.role === "Admin" ? adminColumns : userColumns}
/>
      </Paper>

      <ProductCreateDialog
        key={dialogKey}
        open={openCreate}
        onClose={handleCloseCreateDialog}
        onCreated={handleCloseCreateDialog}
      />

      {selectedProduct && (
        <ProductOrderDialog
          open={openOrder}
          onClose={handleCloseOrderDialog}
          product={selectedProduct}
        />
      )}
    </>
  );
};

export default ProductsPage;