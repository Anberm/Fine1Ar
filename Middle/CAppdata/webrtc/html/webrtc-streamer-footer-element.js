import "./libs/request.min.js";

class WebRTCStreamerFooterElement extends HTMLElement {	
	constructor() {
		super(); 
		this.shadowDOM = this.attachShadow({mode: 'open'});
		this.shadowDOM.innerHTML = `
					<style>@import "styles.css"</style>
					<footer id="footer"></footer>
					`;
	}

	static get observedAttributes() {
		return ['webrtcurl'];
	}  

	attributeChangedCallback(attrName, oldVal, newVal) {
		if (attrName === "webrtcurl") {
			this.fillFooter();
		}
	}

	connectedCallback() {
		this.fillFooter();
	}

	fillFooter() {
		let footerElement = this.shadowDOM.getElementById("footer");
		const webrtcurl = this.getAttribute("webrtcurl") || "";
	
	}

}

customElements.define('webrtc-streamer-footer', WebRTCStreamerFooterElement);
