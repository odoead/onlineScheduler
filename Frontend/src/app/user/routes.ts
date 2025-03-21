import { Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { AuthRedirectComponent } from "./auth-redirect/auth-redirect.component";
import { LogoutRedirectComponent } from "./logout-redirect/logout-redirect.component";
import { authGuard } from "../global/guards/auth.guard";

export const routes: Routes = [
     { path: 'login', component: LoginComponent },
    { path: 'signin-callback', component: AuthRedirectComponent },
    { path: 'signout-callback-oidc', component: LogoutRedirectComponent },
     
    { path: '**', redirectTo: '' }
  ];