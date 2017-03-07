import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import { isBrowser } from 'angular2-universal';
import { AuthenticationService} from '../services/authentication.service';

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(
        private authService: AuthenticationService,
        private router: Router) { }

    canActivate() {
        if (this.authService.loggedIn()) {
            return true;
        }

        this.router.navigate(['/login']);

        return false;
    }
}