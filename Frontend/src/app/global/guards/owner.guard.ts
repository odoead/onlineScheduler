import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const ownerGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
  
    if (authService.isOwnerUser()) {
      return true;
    }
  
    router.navigate(["/forbidden"]);
    return false;
  };