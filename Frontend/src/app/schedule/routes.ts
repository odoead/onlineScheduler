import { Route } from "@angular/router";
import { ScheduleComponent } from "./schedule.component";

export const scheduleRoutes: Route[] = [
    {path: '', component: ScheduleComponent, canActivate: [ ]},
     
]