import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'tab',
    styles: [`
    .pane{
      padding: 1em;
    }
  `],
    templateUrl: './tab.component.html'
})
export class TabComponent implements OnInit {
    
    @Input('tabTitle') title: string;
    @Input() active = false;

    ngOnInit(): void {
        
    }

    activate(value: boolean) {
        this.active = value;
    }
}
