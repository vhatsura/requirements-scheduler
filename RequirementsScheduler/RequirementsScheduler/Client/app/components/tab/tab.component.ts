import { Component, Input, OnInit } from "@angular/core";

@Component({
    selector: "tab",
    styles: [`
    .pane{
      padding: 1em;
    }
  `],
    template: require("./tab.component.html")
})
export class TabComponent implements OnInit {
    ngOnInit(): void {
        
    }

    @Input("tabTitle") title: string;
    @Input() active = false;

    activate(value: boolean) {
        this.active = value;
    }
}
