import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { User, UserManager, WebStorageStateStore } from 'oidc-client-ts';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private userManager!: UserManager;
  private user: User | null = null;

  constructor() {
      this.userManager = new UserManager({
      authority: 'http://localhost:5001',
      client_id: 'angular',
      redirect_uri: 'http://localhost:4200/signin-callback',
      post_logout_redirect_uri: 'http://localhost:4200/signout-callback-oidc',
      response_type: 'code',
      scope: 'openid profile api roles',
      userStore: new WebStorageStateStore({ store: window.localStorage }),
      loadUserInfo: true,
    });

    this.userManager.getUser().then((user) => {
      this.user = user;
    }); 
  }

  login() {
    this.userManager.signinRedirect();
  }

  logout() {
    this.userManager.signoutRedirect();
  }

   async handleCallback() {
    this.user = await this.userManager.signinRedirectCallback();
  }

  isLoggedIn(): Observable<boolean> {
    return of( this.user != null && !this.user.expired);
  }

  isOwnerUser():  Observable<boolean>  {
    if (this.isLoggedIn()) {
      const claims = this.user?.profile['company_role'];
      const roles = Array.isArray(claims) ? claims : [claims];
      return of(roles.some((role: string) => role.startsWith('owner_')));
    }
    return of(false);
  }

  isWorkerUser(): Observable<boolean> {
    if (this.isLoggedIn()) {
      const claims = this.user?.profile['company_role'];
      const roles = Array.isArray(claims) ? claims : [claims];
      return of(roles.some((role: string) => role.startsWith('worker_')));
    }
    return of(false);
  }

  getOwnerCompanyId(): Observable<number | null> {
    if (this.isLoggedIn()) {
      const claims = this.user?.profile['company_role'];
      const roles = Array.isArray(claims) ? claims : [claims];
      const ownerRoles = roles.find((role) => role.startsWith('owner_'));

      return of(ownerRoles ? parseInt(ownerRoles.split('_')[1]) : null);
    }
    return of(null);
  }

  getWorkerCompaniesIds(): Observable<number[]> {
    const workerCompanyIds: number[] = [];
    if (this.isLoggedIn()) {
      const claims = this.user?.profile['company_role'];
      const roles:string[] = Array.isArray(claims) ? claims : [claims];
      const workerRoles = roles.filter((role) => role.startsWith('worker_'));

      workerRoles?.forEach((role) => {
        const companyId = parseInt(role.split('_')[1], 10);
        if (!isNaN(companyId)) {
          workerCompanyIds.push(companyId);
        }
      });
    }
    return of(workerCompanyIds);
  }

  userProfile() {
    return this.user?.profile;
  }
}
