import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../global/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth-redirect',
  standalone: true,
  imports: [],
  templateUrl: './auth-redirect.component.html',
  styleUrl: './auth-redirect.component.css',
})
export class AuthRedirectComponent implements OnInit {
  constructor(private authService: AuthService, private _router: Router) 
  {}
  ngOnInit(): void {
    this.authService
      .handleCallback()
      .then((_) => this._router.navigate(['/'], { replaceUrl: true }));
  }
}
