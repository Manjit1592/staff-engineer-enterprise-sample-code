import { useEffect, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Typography
} from "@mui/material";
import apiClient from "../api/apiClient";
import { getUserFromToken } from "../utils/jwtHelper";

interface Props {
  open: boolean;
  onClose: () => void;
  product: any;
}

const ProductOrderDialog = ({ open, onClose, product }: Props) => {
  const [localOpen, setLocalOpen] = useState(open);
  const [qty, setQty] = useState(1);

  useEffect(() => {
    setLocalOpen(open);
    setQty(1);
  }, [open, product]);

  const handlePlaceOrder = () => {
    const user = getUserFromToken();

    setLocalOpen(false);
    onClose();

    apiClient.post("/orders/place", {
      userId: user?.sub,
      items: [
        {
          productId: product.id,
          productName: product.name,
          unitPrice: product.price,
          quantity: qty
        }
      ]
    }, {
      headers: {
        "Idempotency-Key": crypto.randomUUID()
      }
    })    
    .then((res) => console.log("Order placed", res.data))
    .catch((err) => console.error(err));
  };

  const handleCancel = () => {
    setLocalOpen(false);
    onClose();
    setQty(1);
  };

  return (
    <Dialog open={localOpen} onClose={handleCancel} fullWidth maxWidth="sm">
      <DialogTitle>Place Order</DialogTitle>

      <DialogContent>
        <Typography variant="h6" mb={2}>
          {product?.name}
        </Typography>

        <Typography mb={2}>
          Unit Price: {product?.price}
        </Typography>

        <TextField
          fullWidth
          label="Quantity"
          type="number"
          value={qty}
          onChange={(e) => setQty(Number(e.target.value))}
        />
      </DialogContent>

      <DialogActions>
        <Button onClick={handleCancel}>Cancel</Button>
        <Button variant="contained" onClick={handlePlaceOrder}>
          Confirm Order
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ProductOrderDialog;