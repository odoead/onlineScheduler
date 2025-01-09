import { Component, OnInit } from '@angular/core';
import { AuthService } from '../global/services/auth.service';
import { IdTokenClaims } from 'oidc-client-ts';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
   isLogged:boolean=false;
    profile:IdTokenClaims | undefined=undefined;
    isWorker:boolean= false;
    isOwner:boolean=false;
  constructor(private authService:AuthService) {
     
  }
  ngOnInit(): void {
    this.authService.isLoggedIn().subscribe((loggedIn) => {
      this.isLogged  = loggedIn;
      if (loggedIn) {
        this.profile = this.authService.userProfile();
        this.authService.isWorkerUser().subscribe({next:(status)=>{this.isWorker=status}});
        this.authService.isOwnerUser().subscribe({next:(status)=>{this.isOwner=status}});


       }
    });
  }
  login(): void {
    this.authService.login();
  }

  logout(): void {
    this.authService.logout();
  }
}
