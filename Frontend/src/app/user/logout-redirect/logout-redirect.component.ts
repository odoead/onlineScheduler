import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../global/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-logout-redirect',
  standalone: true,
  imports: [],
  templateUrl: './logout-redirect.component.html',
  styleUrl: './logout-redirect.component.css'
})
export class LogoutRedirectComponent implements OnInit
{

  constructor(private authService: AuthService, private _router: Router) {

  }
  ngOnInit(): void {
    this.authService.logout();
    this._router.navigate(['/'], { replaceUrl: true });
  }

}
