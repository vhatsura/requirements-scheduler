﻿import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { AuthenticationService } from '../../services/authentication.service';

@Component({
    templateUrl: './login.component.html'
})

export class LoginComponent implements OnInit {
    model: any = {};
    loading = false;
    error = '';

    constructor(
        private router: Router,
        private authService: AuthenticationService) { }

    ngOnInit() {
        // reset login status
        this.authService.logout();
    }

    login() {
        this.loading = true;
        console.log(this.model);
        console.log(this.model.username);
        console.log(this.model.password);
        this.authService.login(this.model.username, this.model.password)
            .subscribe(result => {
                if (this.authService.loggedIn()) {
                    this.router.navigate(['/']);
                } else {
                    this.error = 'Username or password is incorrect';
                    this.loading = false;
                }
            });
    }
}
