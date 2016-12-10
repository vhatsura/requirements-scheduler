import { Component, OnInit } from '@angular/core';
import { User } from "../../models/user";
import { UserService } from "../../services/user.service";

@Component({
    selector: 'home',
    template: require('./home.component.html')
})

export class HomeComponent implements OnInit {
    currentUser: User;
    users: User[] = [];

    constructor(private userService: UserService) {
        this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    }

    ngOnInit() {
        this.loadAllUsers();
    }

    private loadAllUsers() {
        this.userService.getAll().subscribe(users => { this.users = users; });
    }
}
