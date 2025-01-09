import { Route } from "@angular/router";
import { ProductComponent } from "./product/product.component";

export const productRoutes: Route[] = [
    {path: '', component: ProductComponent, canActivate: [ ]},
     
]