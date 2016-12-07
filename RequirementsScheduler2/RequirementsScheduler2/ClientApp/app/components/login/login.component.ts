import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import Authenticationservice = require("../../services/authentication.service");
import Alertservice = require("../../services/alert.service");

@Component({
    templateUrl: 'login.component.html'
})

export class LoginComponent implements OnInit {
    model: any = {};
    loading = false;

    constructor(
        private router: Router,
        private authenticationService: Authenticationservice.AuthenticationService,
        private alertService: Alertservice.AlertService) { }

    ngOnInit() {
        // reset login status
        this.authenticationService.logout();
    }

    login() {
        this.loading = true;
        this.authenticationService.login(this.model.username, this.model.password)
            .subscribe(
            data => {
                this.router.navigate(['/']);
            },
            error => {
                this.alertService.error(error);
                this.loading = false;
            });
    }
}