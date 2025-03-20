import { Routes } from '@angular/router';
import { ServerErrorComponent } from './global/server-error/server-error.component';
import { NotFoundComponent } from './global/not-found/not-found.component';
import { HomeComponent } from './home/home.component';
import { ScheduleComponent } from './schedule/schedule.component';
import { ForbiddenComponent } from './global/forbidden/forbidden.component';

export const routes: Routes = [
     {path: '', component: HomeComponent},
   {path: '**', redirectTo: 'not-found', pathMatch: 'full'},
   {path: 'booking', loadChildren: () => import('./bookings/routes').then(r => r.bookingRoutes)},
   {path: 'company', loadChildren: () => import('./company/routes').then(r => r.companyRoutes)},
   {path: 'product', loadChildren: () => import('./product/routes').then(r => r.productRoutes)},
   {path: 'schedule', loadChildren: () => import('./schedule/routes').then(r => r.scheduleRoutes)},
   {path: 'server-error',component:ServerErrorComponent},  
    {path: 'forbidden',component:ForbiddenComponent},
    { path: 'signin-callback', component: HomeComponent },
   {path: 'not-found',component:NotFoundComponent},
   {path:'home',component:HomeComponent},
   {path:'schedule',component:ScheduleComponent}
    
];
/*

export const routes: Routes = [
    {path: '', component: HomeComponent},
    {path: 'shop', component: ShopComponent},
    {path: 'shop/:id', component: ProductDetailsComponent},
    {path: 'cart', component: CartComponent},
    {path: 'checkout', loadChildren: () => import('./features/checkout/routes')
        .then(r => r.checkoutRoutes)},
    {path: 'orders', loadChildren: () => import('./features/orders/routes')
        .then(r => r.orderRoutes)},
    {path: 'account', loadChildren: () => import('./features/account/routes')
        .then(r => r.accountRoutes)},
    {path: 'test-error', component: TestErrorComponent},
    {path: 'not-found', component: NotFoundComponent},
    {path: 'server-error', component: ServerErrorComponent},
    {path: 'admin', loadComponent: () => import('./features/admin/admin.component')
        .then(c => c.AdminComponent), canActivate: [authGuard, adminGuard]},
    {path: '**', redirectTo: 'not-found', pathMatch: 'full'},
];
*/