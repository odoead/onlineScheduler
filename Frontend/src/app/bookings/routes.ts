import { Route } from "@angular/router";
import { BookingsComponent } from "./bookings.component";
import { EditBookingComponent } from "./edit-booking/edit-booking.component";
import { authGuard } from "../global/guards/auth.guard";
import { ownerGuard } from "../global/guards/owner.guard";
import { workerGuard } from "../global/guards/worker.guard";

export const bookingRoutes: Route[] = [
    {path: '', component: BookingsComponent, canActivate: [authGuard,   workerGuard]},
     
]