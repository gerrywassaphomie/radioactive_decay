(() => {
    "use strict";
    const chart = document.getElementById("decay-chart");
    const data = document.getElementById("decay-data");
    if (!chart || !data) return;
    const points = JSON.parse(data.textContent);
    if (!points.length) return;
    const ns = "http://www.w3.org/2000/svg";
    const add = (tag, attributes, text) => {
        const node = document.createElementNS(ns, tag);
        Object.entries(attributes).forEach(([key, value]) => node.setAttribute(key, value));
        if (text !== undefined) node.textContent = text;
        chart.appendChild(node);
        return node;
    };
    const left = 98, right = 730, top = 32, bottom = 290;
    const maxTime = points[points.length - 1].Time || 1;
    const maxQuantity = points[0].RemainingQuantity || 1;
    const x = t => left + (t / maxTime) * (right - left);
    const y = n => bottom - (n / maxQuantity) * (bottom - top);
    const number = n => n.toLocaleString("ru-RU", { maximumSignificantDigits: 4 });
    for (let i = 0; i <= 4; i++) {
        const ratio = i / 4;
        const yy = bottom - ratio * (bottom - top);
        add("line", { x1: left, x2: right, y1: yy, y2: yy, class: "grid-line" });
        add("text", { x: left - 12, y: yy + 4, "text-anchor": "end" }, number(maxQuantity * ratio));
        if (points.length > 1 || i === 0)
            add("text", { x: left + ratio * (right - left), y: bottom + 26, "text-anchor": "middle" }, number(maxTime * ratio));
    }
    add("text", { x: left, y: 17 }, "Количество ядер");
    add("text", { x: right, y: 349, "text-anchor": "end" }, "Время, с");
    add("polyline", { points: points.map(p => x(p.Time) + "," + y(p.RemainingQuantity)).join(" "), class: "decay-line" });
    [points[0], ...(points.length > 1 ? [points[points.length - 1]] : [])].forEach(p =>
        add("circle", { cx: x(p.Time), cy: y(p.RemainingQuantity), r: 5, class: "decay-dot" }));
})();

