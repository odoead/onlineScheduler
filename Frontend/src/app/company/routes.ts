import { Route } from "@angular/router";
import { CompanyComponent } from "./company.component";
import { AdminCompanyComponent } from "./admin-company/admin-company.component";
import { authGuard } from "../global/guards/auth.guard";
import { ownerGuard } from "../global/guards/owner.guard";
import { CreateCompanyComponent } from "./create-company/create-company.component";

export const companyRoutes: Route[] = [
    {path: '', component: CompanyComponent, canActivate: [ ]},
    {path: 'admin', component: AdminCompanyComponent, canActivate: [authGuard, ownerGuard]},
    {path: 'new', component: CreateCompanyComponent, canActivate: [authGuard ]},

]