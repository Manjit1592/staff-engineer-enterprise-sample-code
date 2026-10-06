import { useState } from "react";
import { Box, Button, Container, Paper, TextField, Typography } from "@mui/material";
import apiClient from "../api/apiClient";

interface Props {
  onLoginSuccess: () => void;
}

const LoginPage = ({ onLoginSuccess }: Props) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");

  const handleLogin = async () => {
    try {
      const response = await apiClient.post("/auth/login", {
        email,
        password
      });

      localStorage.setItem("token", response.data.data);
      setMessage("Login Success");
      onLoginSuccess();
    } catch (error: any) {
      setMessage(error.response?.data?.message || "Login Failed");
    }
  };

  return (
    <Container maxWidth="sm" sx={{ mt: 10 }}>
      <Paper elevation={4} sx={{ p: 5 }}>
        <Typography variant="h4" gutterBottom>
          Enterprise Admin Login
        </Typography>

        <TextField
          fullWidth
          label="Email"
          margin="normal"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <TextField
          fullWidth
          label="Password"
          type="password"
          margin="normal"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <Button variant="contained" fullWidth sx={{ mt: 3 }} onClick={handleLogin}>
          Login
        </Button>

        <Box mt={2}>
          <Typography color="primary">{message}</Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default LoginPage;