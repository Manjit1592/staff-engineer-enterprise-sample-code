import {
  AppBar,
  Box,
  CssBaseline,
  Drawer,
  List,
  ListItemButton,
  ListItemText,
  Toolbar,
  Typography
} from "@mui/material";
import { ReactNode } from "react";
import type { UserClaims } from "../utils/jwtHelper";

interface Props {
  children: ReactNode;
  onNavigate: (page: string) => void;
  user: UserClaims | null;
}

const drawerWidth = 240;

const AdminLayout = ({ children, onNavigate, user }: Props) => {
  return (
    <Box sx={{ display: "flex" }}>
      <CssBaseline />

      <AppBar position="fixed" sx={{ zIndex: 1201 }}>
        <Toolbar sx={{ display: "flex", justifyContent: "space-between" }}>
          <Typography variant="h6" fontWeight={700}>
            Enterprise Order Platform Portal
          </Typography>

          <Typography variant="body1">
            {user?.firstName} {user?.lastName} ({user?.role})
          </Typography>
        </Toolbar>
      </AppBar>

      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          [`& .MuiDrawer-paper`]: {
            width: drawerWidth,
            boxSizing: "border-box",
            backgroundColor: "#111827",
            color: "white"
          }
        }}
      >
        <Toolbar />
        <Box sx={{ p: 2 }}>
          <Typography variant="subtitle1" fontWeight={700}>
            {user?.role} MENU
          </Typography>
        </Box>

        <List>
          <ListItemButton onClick={() => onNavigate("dashboard")}>
            <ListItemText primary="Dashboard" />
          </ListItemButton>

          <ListItemButton onClick={() => onNavigate("products")}>
            <ListItemText primary="Products" />
          </ListItemButton>

          {user?.role === "Admin" && (
            <ListItemButton onClick={() => onNavigate("orders")}>
              <ListItemText primary="Orders" />
            </ListItemButton>
          )}

          <ListItemButton
            onClick={() => {
              localStorage.removeItem("token");
              window.location.reload();
            }}
          >
            <ListItemText primary="Logout" />
          </ListItemButton>
        </List>
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, p: 5, mt: 8, backgroundColor: "#f4f6f8", minHeight: "100vh" }}>
        {children}
      </Box>
    </Box>
  );
};

export default AdminLayout;