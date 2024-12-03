import { Component } from '@angular/core';
import { AuthService } from '../../global/services/auth.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

constructor(private authservice:AuthService) {}
login() {
  this.authservice.login();
}
}
