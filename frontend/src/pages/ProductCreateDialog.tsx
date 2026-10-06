import { useEffect, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Stack
} from "@mui/material";
import apiClient from "../api/apiClient";

interface Props {
  open: boolean;
  onClose: () => void;
  onCreated: () => void;
}

const ProductCreateDialog = ({ open, onClose, onCreated }: Props) => {
  const [localOpen, setLocalOpen] = useState(open);

  const [form, setForm] = useState({
    name: "",
    description: "",
    price: "",
    stockQuantity: "",
    sku: ""
  });

  useEffect(() => {
    setLocalOpen(open);
  }, [open]);

  const handleChange = (key: string, value: string) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = () => {
  setLocalOpen(false);
  onClose();

  const payload = {
    name: form.name,
    description: form.description,
    price: Number(form.price),
    stockQuantity: Number(form.stockQuantity),
    sku: form.sku
  };

  setForm({
    name: "",
    description: "",
    price: "",
    stockQuantity: "",
    sku: ""
  });

  apiClient.post("/products/create", payload)
    .then(() => {
      setTimeout(() => {
        onCreated();
      }, 800);
    })
    .catch((error) => {
      console.error("Create issue:", error);
      setTimeout(() => {
        onCreated();
      }, 800);
    });
};

  const handleCancel = () => {
    setLocalOpen(false);
    onClose();
  };

  return (
    <Dialog open={localOpen} onClose={handleCancel} fullWidth maxWidth="sm">
      <DialogTitle>Create Product</DialogTitle>

      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField label="Product Name" value={form.name} onChange={(e) => handleChange("name", e.target.value)} />
          <TextField label="Description" value={form.description} onChange={(e) => handleChange("description", e.target.value)} />
          <TextField label="Price" value={form.price} onChange={(e) => handleChange("price", e.target.value)} />
          <TextField label="Stock Quantity" value={form.stockQuantity} onChange={(e) => handleChange("stockQuantity", e.target.value)} />
          <TextField label="SKU" value={form.sku} onChange={(e) => handleChange("sku", e.target.value)} />
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleCancel}>Cancel</Button>
        <Button variant="contained" onClick={handleSave}>Save</Button>
      </DialogActions>
    </Dialog>
  );
};

export default ProductCreateDialog;